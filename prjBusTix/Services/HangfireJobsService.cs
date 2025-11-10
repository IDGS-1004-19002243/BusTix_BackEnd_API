using Hangfire;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Model;

namespace prjBusTix.Services;

/// <summary>
/// Servicio para programar y ejecutar trabajos en segundo plano con Hangfire
/// </summary>
public class HangfireJobsService
{
    private readonly ILogger<HangfireJobsService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public HangfireJobsService(
        ILogger<HangfireJobsService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Configura todos los trabajos recurrentes de Hangfire
    /// </summary>
    public void ConfigurarTrabajosRecurrentes()
    {
        _logger.LogInformation("Configurando trabajos recurrentes de Hangfire...");

        // Trabajo recurrente: Enviar recordatorios de viajes (cada hora)
        RecurringJob.AddOrUpdate(
            "enviar-recordatorios-viajes",
            () => EnviarRecordatoriosViajesAsync(),
            Cron.Hourly);

        // Trabajo recurrente: Limpiar notificaciones antiguas (diario a las 2 AM)
        RecurringJob.AddOrUpdate(
            "limpiar-notificaciones-antiguas",
            () => LimpiarNotificacionesAntiguasAsync(),
            Cron.Daily(2));

        // Trabajo recurrente: Verificar viajes próximos y enviar recordatorios (cada 30 min)
        RecurringJob.AddOrUpdate(
            "verificar-viajes-proximos",
            () => VerificarViajesProximosAsync(),
            "*/30 * * * *"); // Cada 30 minutos

        _logger.LogInformation("Trabajos recurrentes configurados exitosamente");
    }

    /// <summary>
    /// Programa un recordatorio de viaje específico
    /// </summary>
    public string ProgramarRecordatorioViaje(int viajeId, DateTime fechaEnvio, int horasAntes)
    {
        var jobId = BackgroundJob.Schedule<HangfireJobsService>(
            x => x.EnviarRecordatorioViajeEspecificoAsync(viajeId, horasAntes),
            fechaEnvio);

        _logger.LogInformation(
            "Recordatorio programado para viaje {ViajeId} a las {FechaEnvio}. JobId: {JobId}",
            viajeId, fechaEnvio, jobId);

        return jobId;
    }

    /// <summary>
    /// Envía recordatorios automáticos para viajes próximos
    /// Se ejecuta cada hora
    /// </summary>
    public async Task EnviarRecordatoriosViajesAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notifService = scope.ServiceProvider.GetRequiredService<INotificacionService>();

            var ahora = DateTime.Now;
            var en24Horas = ahora.AddHours(24);

            // Buscar viajes que salgan en las próximas 24 horas
            var viajesProximos = await context.Viajes
                .Where(v => v.FechaSalida >= ahora && v.FechaSalida <= en24Horas)
                .Where(v => v.Estatus == 1) // Solo viajes activos
                .ToListAsync();

            int recordatoriosEnviados = 0;

            foreach (var viaje in viajesProximos)
            {
                var horasHastaSalida = (viaje.FechaSalida - ahora).TotalHours;

                // Enviar recordatorio de 24 horas
                if (horasHastaSalida <= 24 && horasHastaSalida > 23)
                {
                    await notifService.EnviarRecordatorioViajeAsync(viaje.ViajeID, 24);
                    recordatoriosEnviados++;
                }
                // Enviar recordatorio de 4 horas
                else if (horasHastaSalida <= 4 && horasHastaSalida > 3)
                {
                    await notifService.EnviarRecordatorioViajeAsync(viaje.ViajeID, 4);
                    recordatoriosEnviados++;
                }
            }

            _logger.LogInformation(
                "Proceso de recordatorios completado. {Count} recordatorios enviados",
                recordatoriosEnviados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar recordatorios de viajes");
        }
    }

    /// <summary>
    /// Envía un recordatorio específico para un viaje
    /// </summary>
    public async Task EnviarRecordatorioViajeEspecificoAsync(int viajeId, int horasAntes)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var notifService = scope.ServiceProvider.GetRequiredService<INotificacionService>();

            await notifService.EnviarRecordatorioViajeAsync(viajeId, horasAntes);

            _logger.LogInformation(
                "Recordatorio de {Horas} horas enviado para viaje {ViajeId}",
                horasAntes, viajeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error al enviar recordatorio de viaje {ViajeId}", viajeId);
        }
    }

    /// <summary>
    /// Verifica viajes próximos y programa recordatorios automáticos
    /// Se ejecuta cada 30 minutos
    /// </summary>
    public async Task VerificarViajesProximosAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var ahora = DateTime.Now;
            var en48Horas = ahora.AddHours(48);

            // Buscar viajes que salgan en las próximas 48 horas
            var viajesProximos = await context.Viajes
                .Where(v => v.FechaSalida >= ahora && v.FechaSalida <= en48Horas)
                .Where(v => v.Estatus == 1)
                .ToListAsync();

            int viajesProgramados = 0;

            foreach (var viaje in viajesProximos)
            {
                var horasHastaSalida = (viaje.FechaSalida - ahora).TotalHours;

                // Programar recordatorio de 24 horas si aún no se ha enviado
                if (horasHastaSalida > 24 && horasHastaSalida <= 25)
                {
                    var fechaEnvio = viaje.FechaSalida.AddHours(-24);
                    ProgramarRecordatorioViaje(viaje.ViajeID, fechaEnvio, 24);
                    viajesProgramados++;
                }

                // Programar recordatorio de 4 horas si aún no se ha enviado
                if (horasHastaSalida > 4 && horasHastaSalida <= 5)
                {
                    var fechaEnvio = viaje.FechaSalida.AddHours(-4);
                    ProgramarRecordatorioViaje(viaje.ViajeID, fechaEnvio, 4);
                    viajesProgramados++;
                }
            }

            _logger.LogInformation(
                "Verificación de viajes próximos completada. {Count} recordatorios programados",
                viajesProgramados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar viajes próximos");
        }
    }

    /// <summary>
    /// Limpia notificaciones leídas de más de 30 días
    /// Se ejecuta diariamente a las 2 AM
    /// </summary>
    public async Task LimpiarNotificacionesAntiguasAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var hace30Dias = DateTime.Now.AddDays(-30);

            var notificacionesAntiguas = await context.Notificaciones
                .Where(n => n.FueLeida && n.FechaLectura < hace30Dias)
                .ToListAsync();

            if (notificacionesAntiguas.Any())
            {
                context.Notificaciones.RemoveRange(notificacionesAntiguas);
                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "Limpieza de notificaciones completada. {Count} notificaciones eliminadas",
                    notificacionesAntiguas.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar notificaciones antiguas");
        }
    }
}
