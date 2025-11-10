using prjBusTix.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace prjBusTix.Data;

public class AppDbContext : IdentityDbContext<ClApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    // DbSets para todas las tablas del sistema
    public DbSet<EstatusGeneral> EstatusGenerales { get; set; }
    public DbSet<DispositivoUsuario> DispositivosUsuario { get; set; }
    public DbSet<Evento> Eventos { get; set; }
    public DbSet<Unidad> Unidades { get; set; }
    public DbSet<PlantillaRuta> PlantillasRutas { get; set; }
    public DbSet<PlantillaParada> PlantillasParadas { get; set; }
    public DbSet<Viaje> Viajes { get; set; }
    public DbSet<ViajeStaff> ViajesStaff { get; set; }
    public DbSet<ParadaViaje> ParadasViaje { get; set; }
    public DbSet<Cupon> Cupones { get; set; }
    public DbSet<Boleto> Boletos { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<PagoBoleto> PagosBoletos { get; set; }
    public DbSet<ManifiestoPasajero> ManifiestoPasajeros { get; set; }
    public DbSet<RegistroValidacion> RegistroValidacion { get; set; }
    public DbSet<TipoIncidencia> TipoIncidencia { get; set; }
    public DbSet<Incidencia> Incidencias { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }
    public DbSet<AuditoriaCambio> AuditoriaCambios { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // ===== CONFIGURACIÓN ESTATUS GENERAL =====
        modelBuilder.Entity<EstatusGeneral>(entity =>
        {
            entity.HasKey(e => e.Id_Estatus);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.HasIndex(e => e.Categoria);
        });
        
        // ===== CONFIGURACIÓN CLAPPLICATIONUSER =====
        modelBuilder.Entity<ClApplicationUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail);
            entity.HasIndex(e => e.NormalizedUserName);
            
            entity.HasOne(u => u.EstatusNavigation)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.Estatus)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN DISPOSITIVOS USUARIO =====
        modelBuilder.Entity<DispositivoUsuario>(entity =>
        {
            entity.HasKey(e => e.DispositivoID);
            entity.HasIndex(e => e.UsuarioID);
            
            entity.HasOne(d => d.Usuario)
                .WithMany(u => u.Dispositivos)
                .HasForeignKey(d => d.UsuarioID)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ===== CONFIGURACIÓN EVENTOS =====
        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.EventoID);
            entity.HasIndex(e => new { e.Fecha, e.Estatus });
            entity.HasIndex(e => e.Ciudad);
            
            entity.HasOne(e => e.EstatusNavigation)
                .WithMany(s => s.Eventos)
                .HasForeignKey(e => e.Estatus)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Creador)
                .WithMany(u => u.EventosCreados)
                .HasForeignKey(e => e.CreadoPor)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN UNIDADES =====
        modelBuilder.Entity<Unidad>(entity =>
        {
            entity.HasKey(e => e.UnidadID);
            entity.HasIndex(e => e.NumeroEconomico).IsUnique();
            entity.HasIndex(e => e.Placas).IsUnique();
            entity.HasIndex(e => e.Estatus);
            
            entity.HasOne(u => u.EstatusNavigation)
                .WithMany(s => s.Unidades)
                .HasForeignKey(u => u.Estatus)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN PLANTILLAS RUTAS =====
        modelBuilder.Entity<PlantillaRuta>(entity =>
        {
            entity.HasKey(e => e.RutaID);
            entity.HasIndex(e => e.CodigoRuta).IsUnique();
            entity.HasIndex(e => new { e.CiudadOrigen, e.CiudadDestino });
            
            entity.HasOne(r => r.Creador)
                .WithMany(u => u.PlantillasRutasCreadas)
                .HasForeignKey(r => r.CreadoPor)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN PLANTILLAS PARADAS =====
        modelBuilder.Entity<PlantillaParada>(entity =>
        {
            entity.HasKey(e => e.ParadaID);
            entity.HasIndex(e => new { e.PlantillaRutaID, e.OrdenParada });
            
            entity.HasOne(p => p.PlantillaRuta)
                .WithMany(r => r.Paradas)
                .HasForeignKey(p => p.PlantillaRutaID)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ===== CONFIGURACIÓN VIAJES =====
        modelBuilder.Entity<Viaje>(entity =>
        {
            entity.HasKey(e => e.ViajeID);
            entity.HasIndex(e => e.CodigoViaje).IsUnique();
            entity.HasIndex(e => new { e.EventoID, e.FechaSalida });
            entity.HasIndex(e => new { e.FechaSalida, e.Estatus });
            
            entity.HasOne(v => v.Evento)
                .WithMany(e => e.Viajes)
                .HasForeignKey(v => v.EventoID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(v => v.PlantillaRuta)
                .WithMany(r => r.Viajes)
                .HasForeignKey(v => v.PlantillaRutaID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(v => v.Unidad)
                .WithMany(u => u.Viajes)
                .HasForeignKey(v => v.UnidadID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(v => v.Chofer)
                .WithMany(u => u.ViajesComoChofer)
                .HasForeignKey(v => v.ChoferID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(v => v.EstatusNavigation)
                .WithMany(s => s.Viajes)
                .HasForeignKey(v => v.Estatus)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(v => v.Creador)
                .WithMany(u => u.ViajesCreados)
                .HasForeignKey(v => v.CreadoPor)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN VIAJES STAFF =====
        modelBuilder.Entity<ViajeStaff>(entity =>
        {
            entity.HasKey(e => e.AsignacionID);
            entity.HasIndex(e => e.ViajeID);
            entity.HasIndex(e => e.StaffID);
            
            entity.HasOne(vs => vs.Viaje)
                .WithMany(v => v.Staff)
                .HasForeignKey(vs => vs.ViajeID)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(vs => vs.Staff)
                .WithMany(u => u.AsignacionesStaff)
                .HasForeignKey(vs => vs.StaffID)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN PARADAS VIAJE =====
        modelBuilder.Entity<ParadaViaje>(entity =>
        {
            entity.HasKey(e => e.ParadaViajeID);
            entity.HasIndex(e => new { e.ViajeID, e.OrdenParada });
            
            entity.HasOne(pv => pv.Viaje)
                .WithMany(v => v.Paradas)
                .HasForeignKey(pv => pv.ViajeID)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(pv => pv.PlantillaParada)
                .WithMany(pp => pp.ParadasViaje)
                .HasForeignKey(pv => pv.PlantillaParadaID)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN CUPONES =====
        modelBuilder.Entity<Cupon>(entity =>
        {
            entity.HasKey(e => e.CuponID);
            entity.HasIndex(e => e.Codigo).IsUnique();
        });
        
        // ===== CONFIGURACIÓN BOLETOS =====
        modelBuilder.Entity<Boleto>(entity =>
        {
            entity.HasKey(e => e.BoletoID);
            entity.HasIndex(e => e.CodigoBoleto).IsUnique();
            entity.HasIndex(e => e.CodigoQR).IsUnique();
            entity.HasIndex(e => e.ViajeID);
            entity.HasIndex(e => new { e.ClienteID, e.FechaCompra });
            
            entity.HasOne(b => b.Viaje)
                .WithMany(v => v.Boletos)
                .HasForeignKey(b => b.ViajeID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(b => b.Cliente)
                .WithMany(u => u.Boletos)
                .HasForeignKey(b => b.ClienteID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(b => b.ParadaAbordaje)
                .WithMany(p => p.BoletosAbordaje)
                .HasForeignKey(b => b.ParadaAbordajeID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(b => b.EstatusNavigation)
                .WithMany(s => s.Boletos)
                .HasForeignKey(b => b.Estatus)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(b => b.Validador)
                .WithMany(u => u.BoletosValidados)
                .HasForeignKey(b => b.ValidadoPor)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(b => b.CuponAplicado)
                .WithMany(c => c.Boletos)
                .HasForeignKey(b => b.CuponAplicadoID)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN PAGOS =====
        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.PagoID);
            entity.HasIndex(e => e.CodigoPago).IsUnique();
            entity.HasIndex(e => e.UsuarioID);
            entity.HasIndex(e => e.FechaPago).IsDescending();
            
            entity.HasOne(p => p.Usuario)
                .WithMany(u => u.Pagos)
                .HasForeignKey(p => p.UsuarioID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(p => p.EstatusNavigation)
                .WithMany(s => s.Pagos)
                .HasForeignKey(p => p.Estatus)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN PAGOS BOLETOS =====
        modelBuilder.Entity<PagoBoleto>(entity =>
        {
            entity.HasKey(e => e.PagoBoletoID);
            entity.HasIndex(e => new { e.PagoID, e.BoletoID }).IsUnique();
            entity.HasIndex(e => e.PagoID);
            entity.HasIndex(e => e.BoletoID);
            
            entity.HasOne(pb => pb.Pago)
                .WithMany(p => p.PagosBoletos)
                .HasForeignKey(pb => pb.PagoID)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(pb => pb.Boleto)
                .WithMany(b => b.PagosBoletos)
                .HasForeignKey(pb => pb.BoletoID)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ===== CONFIGURACIÓN MANIFIESTO PASAJEROS =====
        modelBuilder.Entity<ManifiestoPasajero>(entity =>
        {
            entity.HasKey(e => e.ManifiestoID);
            entity.HasIndex(e => e.ViajeID);
            entity.HasIndex(e => e.BoletoID).IsUnique();
            
            entity.HasOne(m => m.Viaje)
                .WithMany(v => v.ManifiestoPasajeros)
                .HasForeignKey(m => m.ViajeID)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(m => m.Boleto)
                .WithOne(b => b.ManifiestoPasajero)
                .HasForeignKey<ManifiestoPasajero>(m => m.BoletoID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(m => m.EstatusAbordajeNavigation)
                .WithMany(s => s.ManifiestoPasajeros)
                .HasForeignKey(m => m.EstatusAbordaje)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(m => m.Validador)
                .WithMany(u => u.ManifiestosValidados)
                .HasForeignKey(m => m.ValidadoPor)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(m => m.ParadaAbordaje)
                .WithMany(p => p.ManifiestoPasajeros)
                .HasForeignKey(m => m.ParadaAbordajeID)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN REGISTRO VALIDACION =====
        modelBuilder.Entity<RegistroValidacion>(entity =>
        {
            entity.HasKey(e => e.ValidacionID);
            entity.HasIndex(e => e.BoletoID);
            entity.HasIndex(e => e.ViajeID);
            
            entity.HasOne(r => r.Boleto)
                .WithMany(b => b.RegistrosValidacion)
                .HasForeignKey(r => r.BoletoID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(r => r.Viaje)
                .WithMany(v => v.RegistrosValidacion)
                .HasForeignKey(r => r.ViajeID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(r => r.Validador)
                .WithMany(u => u.Validaciones)
                .HasForeignKey(r => r.ValidadoPor)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN TIPO INCIDENCIA =====
        modelBuilder.Entity<TipoIncidencia>(entity =>
        {
            entity.HasKey(e => e.TipoIncidenciaID);
            entity.HasIndex(e => e.Codigo).IsUnique();
        });
        
        // ===== CONFIGURACIÓN INCIDENCIAS =====
        modelBuilder.Entity<Incidencia>(entity =>
        {
            entity.HasKey(e => e.IncidenciaID);
            entity.HasIndex(e => e.CodigoIncidencia).IsUnique();
            entity.HasIndex(e => e.ViajeID);
            entity.HasIndex(e => e.Estatus);
            
            entity.HasOne(i => i.TipoIncidencia)
                .WithMany(t => t.Incidencias)
                .HasForeignKey(i => i.TipoIncidenciaID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(i => i.Viaje)
                .WithMany(v => v.Incidencias)
                .HasForeignKey(i => i.ViajeID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(i => i.Unidad)
                .WithMany(u => u.Incidencias)
                .HasForeignKey(i => i.UnidadID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(i => i.Reportador)
                .WithMany(u => u.IncidenciasReportadas)
                .HasForeignKey(i => i.ReportadoPor)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(i => i.Asignado)
                .WithMany(u => u.IncidenciasAsignadas)
                .HasForeignKey(i => i.AsignadoA)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(i => i.EstatusNavigation)
                .WithMany(s => s.Incidencias)
                .HasForeignKey(i => i.Estatus)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN NOTIFICACIONES =====
        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.NotificacionID);
            entity.HasIndex(e => new { e.UsuarioID, e.FechaCreacion }).IsDescending();
            entity.HasIndex(e => new { e.UsuarioID, e.FueLeida })
                .HasFilter("[FueLeida] = 0");
            
            entity.HasOne(n => n.Usuario)
                .WithMany(u => u.Notificaciones)
                .HasForeignKey(n => n.UsuarioID)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(n => n.Viaje)
                .WithMany(v => v.Notificaciones)
                .HasForeignKey(n => n.ViajeID)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(n => n.Boleto)
                .WithMany(b => b.Notificaciones)
                .HasForeignKey(n => n.BoletoID)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== CONFIGURACIÓN AUDITORIA CAMBIOS =====
        modelBuilder.Entity<AuditoriaCambio>(entity =>
        {
            entity.HasKey(e => e.AuditoriaID);
            entity.HasIndex(e => new { e.TablaAfectada, e.FechaHoraCambio }).IsDescending();
            entity.HasIndex(e => new { e.TablaAfectada, e.RegistroID, e.FechaHoraCambio }).IsDescending();
            
            entity.HasOne(a => a.Usuario)
                .WithMany(u => u.CambiosAuditados)
                .HasForeignKey(a => a.UsuarioID)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ===== DATOS INICIALES =====
        SeedData(modelBuilder);
    }
    
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Estatus del sistema (Color se maneja en el frontend)
        modelBuilder.Entity<EstatusGeneral>().HasData(
            // Usuario
            new EstatusGeneral { Id_Estatus = 1, Codigo = "USR_ACTIVO", Nombre = "Activo", Descripcion = "Usuario activo en el sistema", Categoria = "Usuario", Orden = 1 },
            new EstatusGeneral { Id_Estatus = 2, Codigo = "USR_INACTIVO", Nombre = "Inactivo", Descripcion = "Usuario inactivo temporalmente", Categoria = "Usuario", Orden = 2 },
            new EstatusGeneral { Id_Estatus = 3, Codigo = "USR_BLOQUEADO", Nombre = "Bloqueado", Descripcion = "Usuario bloqueado por seguridad", Categoria = "Usuario", Orden = 3 },
            
            // Viaje
            new EstatusGeneral { Id_Estatus = 4, Codigo = "VJE_BORRADOR", Nombre = "Borrador", Descripcion = "Viaje en proceso de creación", Categoria = "Viaje", Orden = 1 },
            new EstatusGeneral { Id_Estatus = 5, Codigo = "VJE_PROGRAMADO", Nombre = "Programado", Descripcion = "Viaje confirmado y programado", Categoria = "Viaje", Orden = 2 },
            new EstatusGeneral { Id_Estatus = 6, Codigo = "VJE_EN_CURSO", Nombre = "En Curso", Descripcion = "Viaje en ejecución", Categoria = "Viaje", Orden = 3 },
            new EstatusGeneral { Id_Estatus = 7, Codigo = "VJE_FINALIZADO", Nombre = "Finalizado", Descripcion = "Viaje completado exitosamente", Categoria = "Viaje", Orden = 4 },
            new EstatusGeneral { Id_Estatus = 8, Codigo = "VJE_CANCELADO", Nombre = "Cancelado", Descripcion = "Viaje cancelado", Categoria = "Viaje", Orden = 5 },
            
            // Boleto
            new EstatusGeneral { Id_Estatus = 9, Codigo = "BOL_PENDIENTE", Nombre = "Pendiente", Descripcion = "Boleto reservado pendiente de pago", Categoria = "Boleto", Orden = 1 },
            new EstatusGeneral { Id_Estatus = 10, Codigo = "BOL_PAGADO", Nombre = "Pagado", Descripcion = "Boleto pagado", Categoria = "Boleto", Orden = 2 },
            new EstatusGeneral { Id_Estatus = 11, Codigo = "BOL_VALIDADO", Nombre = "Validado", Descripcion = "Boleto validado para abordaje", Categoria = "Boleto", Orden = 3 },
            new EstatusGeneral { Id_Estatus = 12, Codigo = "BOL_USADO", Nombre = "Usado", Descripcion = "Boleto usado (pasajero abordó)", Categoria = "Boleto", Orden = 4 },
            new EstatusGeneral { Id_Estatus = 13, Codigo = "BOL_CANCELADO", Nombre = "Cancelado", Descripcion = "Boleto cancelado", Categoria = "Boleto", Orden = 5 },
            
            // Pago
            new EstatusGeneral { Id_Estatus = 14, Codigo = "PAG_PENDIENTE", Nombre = "Pendiente", Descripcion = "Pago pendiente de procesar", Categoria = "Pago", Orden = 1 },
            new EstatusGeneral { Id_Estatus = 15, Codigo = "PAG_CAPTURADO", Nombre = "Capturado", Descripcion = "Pago procesado exitosamente", Categoria = "Pago", Orden = 2 },
            new EstatusGeneral { Id_Estatus = 16, Codigo = "PAG_RECHAZADO", Nombre = "Rechazado", Descripcion = "Pago rechazado por pasarela", Categoria = "Pago", Orden = 3 },
            
            // Incidencia
            new EstatusGeneral { Id_Estatus = 17, Codigo = "INC_ABIERTA", Nombre = "Abierta", Descripcion = "Incidencia reportada", Categoria = "Incidencia", Orden = 1 },
            new EstatusGeneral { Id_Estatus = 18, Codigo = "INC_EN_PROCESO", Nombre = "En Proceso", Descripcion = "Incidencia en atención", Categoria = "Incidencia", Orden = 2 },
            new EstatusGeneral { Id_Estatus = 19, Codigo = "INC_RESUELTA", Nombre = "Resuelta", Descripcion = "Incidencia resuelta", Categoria = "Incidencia", Orden = 3 },
            new EstatusGeneral { Id_Estatus = 20, Codigo = "INC_CERRADA", Nombre = "Cerrada", Descripcion = "Incidencia cerrada", Categoria = "Incidencia", Orden = 4 },
            
            // Validacion/Abordaje
            new EstatusGeneral { Id_Estatus = 21, Codigo = "ABD_PENDIENTE", Nombre = "Pendiente", Descripcion = "Pasajero pendiente de abordar", Categoria = "Validacion", Orden = 1 },
            new EstatusGeneral { Id_Estatus = 22, Codigo = "ABD_ABORDADO", Nombre = "Abordado", Descripcion = "Pasajero abordó la unidad", Categoria = "Validacion", Orden = 2 },
            new EstatusGeneral { Id_Estatus = 23, Codigo = "ABD_NO_PRESENTO", Nombre = "No Presentó", Descripcion = "Pasajero no se presentó", Categoria = "Validacion", Orden = 3 },
            
            // Unidad
            new EstatusGeneral { Id_Estatus = 24, Codigo = "UNI_DISPONIBLE", Nombre = "Disponible", Descripcion = "Unidad disponible para asignar", Categoria = "Unidad", Orden = 1 },
            new EstatusGeneral { Id_Estatus = 25, Codigo = "UNI_EN_SERVICIO", Nombre = "En Servicio", Descripcion = "Unidad en servicio activo", Categoria = "Unidad", Orden = 2 },
            new EstatusGeneral { Id_Estatus = 26, Codigo = "UNI_MANTENIMIENTO", Nombre = "Mantenimiento", Descripcion = "Unidad en mantenimiento", Categoria = "Unidad", Orden = 3 },
            
            // Evento
            new EstatusGeneral { Id_Estatus = 27, Codigo = "EVT_PROGRAMADO", Nombre = "Programado", Descripcion = "Evento programado", Categoria = "Evento", Orden = 1 },
            new EstatusGeneral { Id_Estatus = 28, Codigo = "EVT_EN_CURSO", Nombre = "En Curso", Descripcion = "Evento en desarrollo", Categoria = "Evento", Orden = 2 },
            new EstatusGeneral { Id_Estatus = 29, Codigo = "EVT_FINALIZADO", Nombre = "Finalizado", Descripcion = "Evento finalizado", Categoria = "Evento", Orden = 3 },
            new EstatusGeneral { Id_Estatus = 30, Codigo = "EVT_CANCELADO", Nombre = "Cancelado", Descripcion = "Evento cancelado", Categoria = "Evento", Orden = 4 }
        );
        
        // Tipos de incidencia básicos
        modelBuilder.Entity<TipoIncidencia>().HasData(
            new TipoIncidencia { TipoIncidenciaID = 1, Codigo = "INC_MECANICA", Nombre = "Falla Mecánica", Categoria = "Mecanica", Prioridad = "Alta", EsActivo = true },
            new TipoIncidencia { TipoIncidenciaID = 2, Codigo = "INC_RETRASO", Nombre = "Retraso", Categoria = "Operativa", Prioridad = "Media", EsActivo = true },
            new TipoIncidencia { TipoIncidenciaID = 3, Codigo = "INC_ACCIDENTE", Nombre = "Accidente", Categoria = "Seguridad", Prioridad = "Critica", EsActivo = true },
            new TipoIncidencia { TipoIncidenciaID = 4, Codigo = "INC_CLIENTE", Nombre = "Problema con Cliente", Categoria = "Cliente", Prioridad = "Media", EsActivo = true },
            new TipoIncidencia { TipoIncidenciaID = 5, Codigo = "INC_TRAFICO", Nombre = "Tráfico", Categoria = "Operativa", Prioridad = "Baja", EsActivo = true },
            new TipoIncidencia { TipoIncidenciaID = 6, Codigo = "INC_CLIMA", Nombre = "Condiciones Climáticas", Categoria = "Operativa", Prioridad = "Media", EsActivo = true },
            new TipoIncidencia { TipoIncidenciaID = 7, Codigo = "INC_DOCUMENTACION", Nombre = "Documentación Faltante", Categoria = "Administrativa", Prioridad = "Baja", EsActivo = true },
            new TipoIncidencia { TipoIncidenciaID = 8, Codigo = "INC_LIMPIEZA", Nombre = "Problema de Limpieza", Categoria = "Servicio", Prioridad = "Baja", EsActivo = true }
        );
    }
}


