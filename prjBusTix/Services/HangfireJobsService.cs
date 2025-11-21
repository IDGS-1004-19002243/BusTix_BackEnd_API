using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using prjBusTix.Data;

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

        // Solo verificamos viajes próximos para programar sus recordatorios
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

    // NOTA: Se eliminó EnviarRecordatoriosViajesAsync (estrategia de sondeo) 
    // para evitar duplicidad con la estrategia de agendamiento (VerificarViajesProximosAsync).

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

                // Programar recordatorio de 24 horas (ventana 25..24]
                if (horasHastaSalida > 24 && horasHastaSalida <= 25)
                {
                    var fechaEnvio = viaje.FechaSalida.AddHours(-24);
                    ProgramarRecordatorioViaje(viaje.ViajeID, fechaEnvio, 24);
                    viajesProgramados++;
                }

                // Programar recordatorio de 4 horas (ventana 5..4]
                if (horasHastaSalida > 4 && horasHastaSalida <= 5)
                {
                    var fechaEnvio = viaje.FechaSalida.AddHours(-4);
                    ProgramarRecordatorioViaje(viaje.ViajeID, fechaEnvio, 4);
                    viajesProgramados++;
                }

                // Programar recordatorio de 2 horas (ventana 3..2]
                if (horasHastaSalida > 2 && horasHastaSalida <= 3)
                {
                    var fechaEnvio = viaje.FechaSalida.AddHours(-2);
                    ProgramarRecordatorioViaje(viaje.ViajeID, fechaEnvio, 2);
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
}
