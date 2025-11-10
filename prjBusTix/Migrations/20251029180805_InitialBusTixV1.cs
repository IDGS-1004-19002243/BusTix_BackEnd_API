using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace prjBusTix.Migrations
{
    /// <inheritdoc />
    public partial class InitialBusTixV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cupones",
                columns: table => new
                {
                    CuponID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TipoDescuento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ValorDescuento = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    UsosMaximos = table.Column<int>(type: "int", nullable: true),
                    UsosRealizados = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cupones", x => x.CuponID);
                });

            migrationBuilder.CreateTable(
                name: "Estatus_General",
                columns: table => new
                {
                    Id_Estatus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estatus_General", x => x.Id_Estatus);
                });

            migrationBuilder.CreateTable(
                name: "TipoIncidencia",
                columns: table => new
                {
                    TipoIncidenciaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Prioridad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoIncidencia", x => x.TipoIncidenciaID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodigoPostal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NotificacionesPush = table.Column<bool>(type: "bit", nullable: false),
                    NotificacionesEmail = table.Column<bool>(type: "bit", nullable: false),
                    UrlFotoPerfil = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimaConexion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumeroEmpleado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Estatus_General_Estatus",
                        column: x => x.Estatus,
                        principalTable: "Estatus_General",
                        principalColumn: "Id_Estatus",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Unidades",
                columns: table => new
                {
                    UnidadID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroEconomico = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Placas = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Modelo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Año = table.Column<int>(type: "int", nullable: true),
                    TipoUnidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CapacidadAsientos = table.Column<int>(type: "int", nullable: false),
                    TieneClimatizacion = table.Column<bool>(type: "bit", nullable: false),
                    TieneBaño = table.Column<bool>(type: "bit", nullable: false),
                    TieneWifi = table.Column<bool>(type: "bit", nullable: false),
                    UrlFoto = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unidades", x => x.UnidadID);
                    table.ForeignKey(
                        name: "FK_Unidades_Estatus_General_Estatus",
                        column: x => x.Estatus,
                        principalTable: "Estatus_General",
                        principalColumn: "Id_Estatus",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditoriaCambios",
                columns: table => new
                {
                    AuditoriaID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TablaAfectada = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegistroID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoOperacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsuarioID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ValoresAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValoresNuevos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaHoraCambio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaCambios", x => x.AuditoriaID);
                    table.ForeignKey(
                        name: "FK_AuditoriaCambios_AspNetUsers_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispositivosUsuario",
                columns: table => new
                {
                    DispositivoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TokenPush = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoDispositivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Plataforma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispositivosUsuario", x => x.DispositivoID);
                    table.ForeignKey(
                        name: "FK_DispositivosUsuario_AspNetUsers_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Eventos",
                columns: table => new
                {
                    EventoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoEvento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fecha = table.Column<DateTime>(type: "date", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: true),
                    Recinto = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UbicacionLat = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    UbicacionLong = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    UrlImagen = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eventos", x => x.EventoID);
                    table.ForeignKey(
                        name: "FK_Eventos_AspNetUsers_CreadoPor",
                        column: x => x.CreadoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Eventos_Estatus_General_Estatus",
                        column: x => x.Estatus,
                        principalTable: "Estatus_General",
                        principalColumn: "Id_Estatus",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    PagoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransaccionID = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UsuarioID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.PagoID);
                    table.ForeignKey(
                        name: "FK_Pagos_AspNetUsers_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_Estatus_General_Estatus",
                        column: x => x.Estatus,
                        principalTable: "Estatus_General",
                        principalColumn: "Id_Estatus",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlantillasRutas",
                columns: table => new
                {
                    RutaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoRuta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreRuta = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CiudadOrigen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CiudadDestino = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PuntoPartidaLat = table.Column<decimal>(type: "decimal(10,8)", nullable: false),
                    PuntoPartidaLong = table.Column<decimal>(type: "decimal(11,8)", nullable: false),
                    PuntoPartidaNombre = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PuntoLlegadaLat = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    PuntoLlegadaLong = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    PuntoLlegadaNombre = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DistanciaKm = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    TiempoEstimadoMinutos = table.Column<int>(type: "int", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasRutas", x => x.RutaID);
                    table.ForeignKey(
                        name: "FK_PlantillasRutas_AspNetUsers_CreadoPor",
                        column: x => x.CreadoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlantillasParadas",
                columns: table => new
                {
                    ParadaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantillaRutaID = table.Column<int>(type: "int", nullable: false),
                    NombreParada = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Latitud = table.Column<decimal>(type: "decimal(10,8)", nullable: false),
                    Longitud = table.Column<decimal>(type: "decimal(11,8)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OrdenParada = table.Column<int>(type: "int", nullable: false),
                    TiempoEsperaMinutos = table.Column<int>(type: "int", nullable: false),
                    EsActiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasParadas", x => x.ParadaID);
                    table.ForeignKey(
                        name: "FK_PlantillasParadas_PlantillasRutas_PlantillaRutaID",
                        column: x => x.PlantillaRutaID,
                        principalTable: "PlantillasRutas",
                        principalColumn: "RutaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Viajes",
                columns: table => new
                {
                    ViajeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventoID = table.Column<int>(type: "int", nullable: false),
                    PlantillaRutaID = table.Column<int>(type: "int", nullable: false),
                    UnidadID = table.Column<int>(type: "int", nullable: true),
                    ChoferID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CodigoViaje = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TipoViaje = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaSalida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaLlegadaEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CupoTotal = table.Column<int>(type: "int", nullable: false),
                    AsientosDisponibles = table.Column<int>(type: "int", nullable: false),
                    AsientosVendidos = table.Column<int>(type: "int", nullable: false),
                    PrecioBase = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CargoServicio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    VentasAbiertas = table.Column<bool>(type: "bit", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Viajes", x => x.ViajeID);
                    table.ForeignKey(
                        name: "FK_Viajes_AspNetUsers_ChoferID",
                        column: x => x.ChoferID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Viajes_AspNetUsers_CreadoPor",
                        column: x => x.CreadoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Viajes_Estatus_General_Estatus",
                        column: x => x.Estatus,
                        principalTable: "Estatus_General",
                        principalColumn: "Id_Estatus",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Viajes_Eventos_EventoID",
                        column: x => x.EventoID,
                        principalTable: "Eventos",
                        principalColumn: "EventoID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Viajes_PlantillasRutas_PlantillaRutaID",
                        column: x => x.PlantillaRutaID,
                        principalTable: "PlantillasRutas",
                        principalColumn: "RutaID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Viajes_Unidades_UnidadID",
                        column: x => x.UnidadID,
                        principalTable: "Unidades",
                        principalColumn: "UnidadID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Incidencias",
                columns: table => new
                {
                    IncidenciaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoIncidencia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TipoIncidenciaID = table.Column<int>(type: "int", nullable: false),
                    ViajeID = table.Column<int>(type: "int", nullable: true),
                    UnidadID = table.Column<int>(type: "int", nullable: true),
                    ReportadoPor = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prioridad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaReporte = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    AsignadoA = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidencias", x => x.IncidenciaID);
                    table.ForeignKey(
                        name: "FK_Incidencias_AspNetUsers_AsignadoA",
                        column: x => x.AsignadoA,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidencias_AspNetUsers_ReportadoPor",
                        column: x => x.ReportadoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidencias_Estatus_General_Estatus",
                        column: x => x.Estatus,
                        principalTable: "Estatus_General",
                        principalColumn: "Id_Estatus",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidencias_TipoIncidencia_TipoIncidenciaID",
                        column: x => x.TipoIncidenciaID,
                        principalTable: "TipoIncidencia",
                        principalColumn: "TipoIncidenciaID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidencias_Unidades_UnidadID",
                        column: x => x.UnidadID,
                        principalTable: "Unidades",
                        principalColumn: "UnidadID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidencias_Viajes_ViajeID",
                        column: x => x.ViajeID,
                        principalTable: "Viajes",
                        principalColumn: "ViajeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParadasViaje",
                columns: table => new
                {
                    ParadaViajeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViajeID = table.Column<int>(type: "int", nullable: false),
                    PlantillaParadaID = table.Column<int>(type: "int", nullable: true),
                    NombreParada = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Latitud = table.Column<decimal>(type: "decimal(10,8)", nullable: false),
                    Longitud = table.Column<decimal>(type: "decimal(11,8)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OrdenParada = table.Column<int>(type: "int", nullable: false),
                    HoraEstimadaLlegada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TiempoEsperaMinutos = table.Column<int>(type: "int", nullable: false),
                    EsActiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParadasViaje", x => x.ParadaViajeID);
                    table.ForeignKey(
                        name: "FK_ParadasViaje_PlantillasParadas_PlantillaParadaID",
                        column: x => x.PlantillaParadaID,
                        principalTable: "PlantillasParadas",
                        principalColumn: "ParadaID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParadasViaje_Viajes_ViajeID",
                        column: x => x.ViajeID,
                        principalTable: "Viajes",
                        principalColumn: "ViajeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViajesStaff",
                columns: table => new
                {
                    AsignacionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViajeID = table.Column<int>(type: "int", nullable: false),
                    StaffID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RolEnViaje = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViajesStaff", x => x.AsignacionID);
                    table.ForeignKey(
                        name: "FK_ViajesStaff_AspNetUsers_StaffID",
                        column: x => x.StaffID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViajesStaff_Viajes_ViajeID",
                        column: x => x.ViajeID,
                        principalTable: "Viajes",
                        principalColumn: "ViajeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Boletos",
                columns: table => new
                {
                    BoletoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViajeID = table.Column<int>(type: "int", nullable: false),
                    ClienteID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CodigoBoleto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodigoQR = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NumeroAsiento = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NombrePasajero = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EmailPasajero = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TelefonoPasajero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ParadaAbordajeID = table.Column<int>(type: "int", nullable: true),
                    PrecioBase = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CargoServicio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IVA = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CuponAplicadoID = table.Column<int>(type: "int", nullable: true),
                    FechaCompra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    FechaValidacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidadoPor = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boletos", x => x.BoletoID);
                    table.ForeignKey(
                        name: "FK_Boletos_AspNetUsers_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Boletos_AspNetUsers_ValidadoPor",
                        column: x => x.ValidadoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Boletos_Cupones_CuponAplicadoID",
                        column: x => x.CuponAplicadoID,
                        principalTable: "Cupones",
                        principalColumn: "CuponID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Boletos_Estatus_General_Estatus",
                        column: x => x.Estatus,
                        principalTable: "Estatus_General",
                        principalColumn: "Id_Estatus",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Boletos_ParadasViaje_ParadaAbordajeID",
                        column: x => x.ParadaAbordajeID,
                        principalTable: "ParadasViaje",
                        principalColumn: "ParadaViajeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Boletos_Viajes_ViajeID",
                        column: x => x.ViajeID,
                        principalTable: "Viajes",
                        principalColumn: "ViajeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManifiestoPasajeros",
                columns: table => new
                {
                    ManifiestoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViajeID = table.Column<int>(type: "int", nullable: false),
                    BoletoID = table.Column<int>(type: "int", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NumeroAsiento = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ParadaAbordajeID = table.Column<int>(type: "int", nullable: true),
                    EstatusAbordaje = table.Column<int>(type: "int", nullable: false),
                    FechaAbordaje = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FueValidado = table.Column<bool>(type: "bit", nullable: false),
                    FechaValidacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidadoPor = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManifiestoPasajeros", x => x.ManifiestoID);
                    table.ForeignKey(
                        name: "FK_ManifiestoPasajeros_AspNetUsers_ValidadoPor",
                        column: x => x.ValidadoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManifiestoPasajeros_Boletos_BoletoID",
                        column: x => x.BoletoID,
                        principalTable: "Boletos",
                        principalColumn: "BoletoID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManifiestoPasajeros_Estatus_General_EstatusAbordaje",
                        column: x => x.EstatusAbordaje,
                        principalTable: "Estatus_General",
                        principalColumn: "Id_Estatus",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManifiestoPasajeros_ParadasViaje_ParadaAbordajeID",
                        column: x => x.ParadaAbordajeID,
                        principalTable: "ParadasViaje",
                        principalColumn: "ParadaViajeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManifiestoPasajeros_Viajes_ViajeID",
                        column: x => x.ViajeID,
                        principalTable: "Viajes",
                        principalColumn: "ViajeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    NotificacionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ViajeID = table.Column<int>(type: "int", nullable: true),
                    BoletoID = table.Column<int>(type: "int", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoNotificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnviarPush = table.Column<bool>(type: "bit", nullable: false),
                    EnviarEmail = table.Column<bool>(type: "bit", nullable: false),
                    FueEnviada = table.Column<bool>(type: "bit", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FueLeida = table.Column<bool>(type: "bit", nullable: false),
                    FechaLectura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.NotificacionID);
                    table.ForeignKey(
                        name: "FK_Notificaciones_AspNetUsers_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Boletos_BoletoID",
                        column: x => x.BoletoID,
                        principalTable: "Boletos",
                        principalColumn: "BoletoID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Viajes_ViajeID",
                        column: x => x.ViajeID,
                        principalTable: "Viajes",
                        principalColumn: "ViajeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagosBoletos",
                columns: table => new
                {
                    PagoBoletoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PagoID = table.Column<int>(type: "int", nullable: false),
                    BoletoID = table.Column<int>(type: "int", nullable: false),
                    MontoAsignado = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosBoletos", x => x.PagoBoletoID);
                    table.ForeignKey(
                        name: "FK_PagosBoletos_Boletos_BoletoID",
                        column: x => x.BoletoID,
                        principalTable: "Boletos",
                        principalColumn: "BoletoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PagosBoletos_Pagos_PagoID",
                        column: x => x.PagoID,
                        principalTable: "Pagos",
                        principalColumn: "PagoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistroValidacion",
                columns: table => new
                {
                    ValidacionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoletoID = table.Column<int>(type: "int", nullable: false),
                    ViajeID = table.Column<int>(type: "int", nullable: false),
                    ValidadoPor = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CodigoQREscaneado = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResultadoValidacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaHoraValidacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModoOffline = table.Column<bool>(type: "bit", nullable: false),
                    FechaSincronizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistroValidacion", x => x.ValidacionID);
                    table.ForeignKey(
                        name: "FK_RegistroValidacion_AspNetUsers_ValidadoPor",
                        column: x => x.ValidadoPor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistroValidacion_Boletos_BoletoID",
                        column: x => x.BoletoID,
                        principalTable: "Boletos",
                        principalColumn: "BoletoID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistroValidacion_Viajes_ViajeID",
                        column: x => x.ViajeID,
                        principalTable: "Viajes",
                        principalColumn: "ViajeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Estatus_General",
                columns: new[] { "Id_Estatus", "Categoria", "Codigo", "Descripcion", "EsActivo", "Nombre", "Orden" },
                values: new object[,]
                {
                    { 1, "Usuario", "USR_ACTIVO", "Usuario activo en el sistema", true, "Activo", 1 },
                    { 2, "Usuario", "USR_INACTIVO", "Usuario inactivo temporalmente", true, "Inactivo", 2 },
                    { 3, "Usuario", "USR_BLOQUEADO", "Usuario bloqueado por seguridad", true, "Bloqueado", 3 },
                    { 4, "Viaje", "VJE_BORRADOR", "Viaje en proceso de creación", true, "Borrador", 1 },
                    { 5, "Viaje", "VJE_PROGRAMADO", "Viaje confirmado y programado", true, "Programado", 2 },
                    { 6, "Viaje", "VJE_EN_CURSO", "Viaje en ejecución", true, "En Curso", 3 },
                    { 7, "Viaje", "VJE_FINALIZADO", "Viaje completado exitosamente", true, "Finalizado", 4 },
                    { 8, "Viaje", "VJE_CANCELADO", "Viaje cancelado", true, "Cancelado", 5 },
                    { 9, "Boleto", "BOL_PENDIENTE", "Boleto reservado pendiente de pago", true, "Pendiente", 1 },
                    { 10, "Boleto", "BOL_PAGADO", "Boleto pagado", true, "Pagado", 2 },
                    { 11, "Boleto", "BOL_VALIDADO", "Boleto validado para abordaje", true, "Validado", 3 },
                    { 12, "Boleto", "BOL_USADO", "Boleto usado (pasajero abordó)", true, "Usado", 4 },
                    { 13, "Boleto", "BOL_CANCELADO", "Boleto cancelado", true, "Cancelado", 5 },
                    { 14, "Pago", "PAG_PENDIENTE", "Pago pendiente de procesar", true, "Pendiente", 1 },
                    { 15, "Pago", "PAG_CAPTURADO", "Pago procesado exitosamente", true, "Capturado", 2 },
                    { 16, "Pago", "PAG_RECHAZADO", "Pago rechazado por pasarela", true, "Rechazado", 3 },
                    { 17, "Incidencia", "INC_ABIERTA", "Incidencia reportada", true, "Abierta", 1 },
                    { 18, "Incidencia", "INC_EN_PROCESO", "Incidencia en atención", true, "En Proceso", 2 },
                    { 19, "Incidencia", "INC_RESUELTA", "Incidencia resuelta", true, "Resuelta", 3 },
                    { 20, "Incidencia", "INC_CERRADA", "Incidencia cerrada", true, "Cerrada", 4 },
                    { 21, "Validacion", "ABD_PENDIENTE", "Pasajero pendiente de abordar", true, "Pendiente", 1 },
                    { 22, "Validacion", "ABD_ABORDADO", "Pasajero abordó la unidad", true, "Abordado", 2 },
                    { 23, "Validacion", "ABD_NO_PRESENTO", "Pasajero no se presentó", true, "No Presentó", 3 },
                    { 24, "Unidad", "UNI_DISPONIBLE", "Unidad disponible para asignar", true, "Disponible", 1 },
                    { 25, "Unidad", "UNI_EN_SERVICIO", "Unidad en servicio activo", true, "En Servicio", 2 },
                    { 26, "Unidad", "UNI_MANTENIMIENTO", "Unidad en mantenimiento", true, "Mantenimiento", 3 },
                    { 27, "Evento", "EVT_PROGRAMADO", "Evento programado", true, "Programado", 1 },
                    { 28, "Evento", "EVT_EN_CURSO", "Evento en desarrollo", true, "En Curso", 2 },
                    { 29, "Evento", "EVT_FINALIZADO", "Evento finalizado", true, "Finalizado", 3 },
                    { 30, "Evento", "EVT_CANCELADO", "Evento cancelado", true, "Cancelado", 4 }
                });

            migrationBuilder.InsertData(
                table: "TipoIncidencia",
                columns: new[] { "TipoIncidenciaID", "Categoria", "Codigo", "EsActivo", "Nombre", "Prioridad" },
                values: new object[,]
                {
                    { 1, "Mecanica", "INC_MECANICA", true, "Falla Mecánica", "Alta" },
                    { 2, "Operativa", "INC_RETRASO", true, "Retraso", "Media" },
                    { 3, "Seguridad", "INC_ACCIDENTE", true, "Accidente", "Critica" },
                    { 4, "Cliente", "INC_CLIENTE", true, "Problema con Cliente", "Media" },
                    { 5, "Operativa", "INC_TRAFICO", true, "Tráfico", "Baja" },
                    { 6, "Operativa", "INC_CLIMA", true, "Condiciones Climáticas", "Media" },
                    { 7, "Administrativa", "INC_DOCUMENTACION", true, "Documentación Faltante", "Baja" },
                    { 8, "Servicio", "INC_LIMPIEZA", true, "Problema de Limpieza", "Baja" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Estatus",
                table: "AspNetUsers",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaCambios_TablaAfectada_FechaHoraCambio",
                table: "AuditoriaCambios",
                columns: new[] { "TablaAfectada", "FechaHoraCambio" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaCambios_TablaAfectada_RegistroID_FechaHoraCambio",
                table: "AuditoriaCambios",
                columns: new[] { "TablaAfectada", "RegistroID", "FechaHoraCambio" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaCambios_UsuarioID",
                table: "AuditoriaCambios",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_ClienteID_FechaCompra",
                table: "Boletos",
                columns: new[] { "ClienteID", "FechaCompra" });

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_CodigoBoleto",
                table: "Boletos",
                column: "CodigoBoleto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_CodigoQR",
                table: "Boletos",
                column: "CodigoQR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_CuponAplicadoID",
                table: "Boletos",
                column: "CuponAplicadoID");

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_Estatus",
                table: "Boletos",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_ParadaAbordajeID",
                table: "Boletos",
                column: "ParadaAbordajeID");

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_ValidadoPor",
                table: "Boletos",
                column: "ValidadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_ViajeID",
                table: "Boletos",
                column: "ViajeID");

            migrationBuilder.CreateIndex(
                name: "IX_Cupones_Codigo",
                table: "Cupones",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosUsuario_UsuarioID",
                table: "DispositivosUsuario",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Estatus_General_Categoria",
                table: "Estatus_General",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Estatus_General_Codigo",
                table: "Estatus_General",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_Ciudad",
                table: "Eventos",
                column: "Ciudad");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_CreadoPor",
                table: "Eventos",
                column: "CreadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_Estatus",
                table: "Eventos",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_Fecha_Estatus",
                table: "Eventos",
                columns: new[] { "Fecha", "Estatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_AsignadoA",
                table: "Incidencias",
                column: "AsignadoA");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_CodigoIncidencia",
                table: "Incidencias",
                column: "CodigoIncidencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_Estatus",
                table: "Incidencias",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_ReportadoPor",
                table: "Incidencias",
                column: "ReportadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_TipoIncidenciaID",
                table: "Incidencias",
                column: "TipoIncidenciaID");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_UnidadID",
                table: "Incidencias",
                column: "UnidadID");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_ViajeID",
                table: "Incidencias",
                column: "ViajeID");

            migrationBuilder.CreateIndex(
                name: "IX_ManifiestoPasajeros_BoletoID",
                table: "ManifiestoPasajeros",
                column: "BoletoID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManifiestoPasajeros_EstatusAbordaje",
                table: "ManifiestoPasajeros",
                column: "EstatusAbordaje");

            migrationBuilder.CreateIndex(
                name: "IX_ManifiestoPasajeros_ParadaAbordajeID",
                table: "ManifiestoPasajeros",
                column: "ParadaAbordajeID");

            migrationBuilder.CreateIndex(
                name: "IX_ManifiestoPasajeros_ValidadoPor",
                table: "ManifiestoPasajeros",
                column: "ValidadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_ManifiestoPasajeros_ViajeID",
                table: "ManifiestoPasajeros",
                column: "ViajeID");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_BoletoID",
                table: "Notificaciones",
                column: "BoletoID");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioID_FechaCreacion",
                table: "Notificaciones",
                columns: new[] { "UsuarioID", "FechaCreacion" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioID_FueLeida",
                table: "Notificaciones",
                columns: new[] { "UsuarioID", "FueLeida" },
                filter: "[FueLeida] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_ViajeID",
                table: "Notificaciones",
                column: "ViajeID");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_CodigoPago",
                table: "Pagos",
                column: "CodigoPago",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Estatus",
                table: "Pagos",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_FechaPago",
                table: "Pagos",
                column: "FechaPago",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_UsuarioID",
                table: "Pagos",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_PagosBoletos_BoletoID",
                table: "PagosBoletos",
                column: "BoletoID");

            migrationBuilder.CreateIndex(
                name: "IX_PagosBoletos_PagoID",
                table: "PagosBoletos",
                column: "PagoID");

            migrationBuilder.CreateIndex(
                name: "IX_PagosBoletos_PagoID_BoletoID",
                table: "PagosBoletos",
                columns: new[] { "PagoID", "BoletoID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParadasViaje_PlantillaParadaID",
                table: "ParadasViaje",
                column: "PlantillaParadaID");

            migrationBuilder.CreateIndex(
                name: "IX_ParadasViaje_ViajeID_OrdenParada",
                table: "ParadasViaje",
                columns: new[] { "ViajeID", "OrdenParada" });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasParadas_PlantillaRutaID_OrdenParada",
                table: "PlantillasParadas",
                columns: new[] { "PlantillaRutaID", "OrdenParada" });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasRutas_CiudadOrigen_CiudadDestino",
                table: "PlantillasRutas",
                columns: new[] { "CiudadOrigen", "CiudadDestino" });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasRutas_CodigoRuta",
                table: "PlantillasRutas",
                column: "CodigoRuta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasRutas_CreadoPor",
                table: "PlantillasRutas",
                column: "CreadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_RegistroValidacion_BoletoID",
                table: "RegistroValidacion",
                column: "BoletoID");

            migrationBuilder.CreateIndex(
                name: "IX_RegistroValidacion_ValidadoPor",
                table: "RegistroValidacion",
                column: "ValidadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_RegistroValidacion_ViajeID",
                table: "RegistroValidacion",
                column: "ViajeID");

            migrationBuilder.CreateIndex(
                name: "IX_TipoIncidencia_Codigo",
                table: "TipoIncidencia",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_Estatus",
                table: "Unidades",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_NumeroEconomico",
                table: "Unidades",
                column: "NumeroEconomico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_Placas",
                table: "Unidades",
                column: "Placas",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_ChoferID",
                table: "Viajes",
                column: "ChoferID");

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_CodigoViaje",
                table: "Viajes",
                column: "CodigoViaje",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_CreadoPor",
                table: "Viajes",
                column: "CreadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_Estatus",
                table: "Viajes",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_EventoID_FechaSalida",
                table: "Viajes",
                columns: new[] { "EventoID", "FechaSalida" });

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_FechaSalida_Estatus",
                table: "Viajes",
                columns: new[] { "FechaSalida", "Estatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_PlantillaRutaID",
                table: "Viajes",
                column: "PlantillaRutaID");

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_UnidadID",
                table: "Viajes",
                column: "UnidadID");

            migrationBuilder.CreateIndex(
                name: "IX_ViajesStaff_StaffID",
                table: "ViajesStaff",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_ViajesStaff_ViajeID",
                table: "ViajesStaff",
                column: "ViajeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditoriaCambios");

            migrationBuilder.DropTable(
                name: "DispositivosUsuario");

            migrationBuilder.DropTable(
                name: "Incidencias");

            migrationBuilder.DropTable(
                name: "ManifiestoPasajeros");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "PagosBoletos");

            migrationBuilder.DropTable(
                name: "RegistroValidacion");

            migrationBuilder.DropTable(
                name: "ViajesStaff");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "TipoIncidencia");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Boletos");

            migrationBuilder.DropTable(
                name: "Cupones");

            migrationBuilder.DropTable(
                name: "ParadasViaje");

            migrationBuilder.DropTable(
                name: "PlantillasParadas");

            migrationBuilder.DropTable(
                name: "Viajes");

            migrationBuilder.DropTable(
                name: "Eventos");

            migrationBuilder.DropTable(
                name: "PlantillasRutas");

            migrationBuilder.DropTable(
                name: "Unidades");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Estatus_General");
        }
    }
}
