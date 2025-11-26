using System.Text;
using System.Linq;
using System.Collections.Generic;
using Hangfire;
using prjBusTix.Data;
using prjBusTix.Model;
using prjBusTix.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var jwtSettings = builder.Configuration.GetSection("JWTSetting");

// Configuración de CORS
builder.Services.AddCors(options =>
{
    // Política por defecto: aceptar orígenes de desarrollo (localhost), LAN, Netlify y producción
    options.AddDefaultPolicy(policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;
                try
                {
                    var uri = new Uri(origin);

                    // Desarrollo web (localhost en cualquier puerto)
                    if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                        return true;

                    // Desarrollo web (127.0.0.1 en cualquier puerto)
                    if (uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                        return true;

                    // React Native en LAN (celular real o emulador): 192.168.*
                    if (uri.Host.StartsWith("192.168."))
                        return true;

                    // Frontend en Netlify (producción web)
                    if (uri.Host.Contains("netlify.app", StringComparison.OrdinalIgnoreCase))
                        return true;

                    // Dominio de producción en stempurl.com
                    if (uri.Host.Contains("stempurl.com", StringComparison.OrdinalIgnoreCase))
                        return true;

                    // Dominio de producción en site4now.net (por si el backend está ahí también)
                    if (uri.Host.Contains("site4now.net", StringComparison.OrdinalIgnoreCase))
                        return true;

                    return false;
                }
                catch
                {
                    return false;
                }
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("*"); // Exponer todos los headers en las respuestas
    });

    // Política específica para localhost:3000 (opcional, ya está cubierta por la política por defecto)
    options.AddPolicy("Local3000", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configuración de SQL Server
var connectionString = builder.Configuration.GetConnectionString("cadenaSQL");
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// Configurar Hangfire para tareas en segundo plano
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new Hangfire.SqlServer.SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        // Evitar polling agresivo en entornos remotos para reducir la carga y conexiones fallidas
        QueuePollInterval = TimeSpan.FromSeconds(15),
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Agregar servidor de Hangfire (deshabilitado por defecto en desarrollo/para migraciones remotas)
// Si quieres habilitar el servidor en este proceso, establece la variable de entorno ENABLE_HANGFIRE_SERVER=true
if (Environment.GetEnvironmentVariable("ENABLE_HANGFIRE_SERVER") == "true")
{
    builder.Services.AddHangfireServer();
}

builder.Services.AddIdentity<ClApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;

    // Cargar la clave de configuración de JWT de forma robusta
    var securityKey = jwtSettings["securityKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
    if (string.IsNullOrWhiteSpace(securityKey))
    {
        // En desarrollo usamos una clave fallback para permitir arranque local.
        if (builder.Environment.IsDevelopment())
        {
            securityKey = "dev-fallback-key-change-in-production-please";
            Console.WriteLine("[WARN] Se está usando una clave JWT de desarrollo por falta de configuración. No utilizar en producción.");
        }
        else
        {
            // En producción es preferible fallar rápido con un mensaje claro.
            Console.WriteLine("[ERROR] La clave 'JWTSetting:securityKey' no está configurada y no se encontró la variable de entorno 'JWT_SECRET'. La aplicación no puede iniciar de forma segura.");
            throw new InvalidOperationException("La clave 'securityKey' no está configurada en JWTSettings. Configure la sección 'JWTSetting:securityKey' en appsettings o la variable de entorno 'JWT_SECRET'.");
        }
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        // Mantener el issuer configurado y aceptar múltiples audiences (útil durante la transición)
        ValidIssuer = jwtSettings["ValidIssuer"],
        ValidAudiences = new[] { jwtSettings["ValidAudience"], "http://localhost:5289", "https://waldoz-001-site1.stempurl.com" },
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey))
    };
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthAPI", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        
        Description = "Ingresa 'Bearer' [espacio] y luego tu token JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    // Evita excepción de Swashbuckle cuando hay descripciones de API con rutas/métodos duplicados.
    // Selecciona la primera descripción en caso de conflicto. Idealmente las rutas deben ser únicas,
    // pero esto evita que la generación de swagger cause un HTTP 500 en startup.
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    
});

// Registrar autorización por permisos: provider y handler
builder.Services.AddAuthorization();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, prjBusTix.Security.PermissionPolicyProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, prjBusTix.Security.PermissionHandler>();

// Registrar servicios de aplicación
builder.Services.AddScoped<prjBusTix.Services.INotificacionService, prjBusTix.Services.NotificacionService>();
builder.Services.AddScoped<prjBusTix.Services.HangfireJobsService>();
builder.Services.AddScoped<prjBusTix.Services.IAuditoriaService, prjBusTix.Services.AuditoriaService>();
// Registrar servicio de validación


builder.Services.AddHttpContextAccessor(); // Necesario para auditoría

// Configurar SignalR para notificaciones en tiempo real
builder.Services.AddSignalR();

var app = builder.Build();

// Inicializar base de datos y roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ClApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Aplicar migraciones pendientes (usamos Migrate en lugar de EnsureCreated para mantener historial de migraciones)
        context.Database.Migrate();

        // Crear roles iniciales si no existen
        string[] roleNames = { "Admin", "User", "Manager", "Operator", "Staff" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        
        // Crear usuario admin por defecto si no existe
        var adminEmail = "admin@bustix.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ClApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                NombreCompleto = "Administrador del Sistema",
                EmailConfirmed = true,
                Estatus = 1
            };
            
            var result = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Crear usuario staff por defecto para pruebas si no existe
        var staffEmail = "staff@bustix.com";
        var staffUser = await userManager.FindByEmailAsync(staffEmail);
        if (staffUser == null)
        {
            staffUser = new ClApplicationUser
            {
                UserName = staffEmail,
                Email = staffEmail,
                NombreCompleto = "Staff de Pruebas",
                EmailConfirmed = true,
                Estatus = 1
            };

            var resultStaff = await userManager.CreateAsync(staffUser, "Staff@123456");
            if (resultStaff.Succeeded)
            {
                // Asegurar que exista el rol Staff y asignarlo
                if (!await roleManager.RoleExistsAsync("Staff"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Staff"));
                }
                await userManager.AddToRoleAsync(staffUser, "Staff");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthAPI v1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();

// Aplicar CORS (usa la política por defecto que permite localhost:anyport)
app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

// Dashboard de Hangfire (solo accesible por Admin)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new prjBusTix.Security.HangfireAuthorizationFilter() },
    DashboardTitle = "BusTix - Jobs en Segundo Plano"
});

// Configurar jobs recurrentes de Hangfire
using (var scope = app.Services.CreateScope())
{
    var hangfireService = scope.ServiceProvider.GetRequiredService<prjBusTix.Services.HangfireJobsService>();
    hangfireService.ConfigurarTrabajosRecurrentes();
}

// Mapear el Hub de SignalR para notificaciones en tiempo real
app.MapHub<prjBusTix.Hubs.NotificacionesHub>("/hubs/notificaciones");

app.MapControllers();

app.Run();//gnmiolwrxf115661