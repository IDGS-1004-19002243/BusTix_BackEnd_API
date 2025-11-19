using prjBusTix.Dto.Boletos;

namespace prjBusTix.Services;

public interface IValidacionService
{
    Task<SincronizacionResponseDto> ProcesarValidacionesAsync(List<ValidacionSyncDto> validaciones, string staffId);
}
