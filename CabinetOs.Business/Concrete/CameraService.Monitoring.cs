using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Camera.Commands;
using DeviceStatusEnum = CabinetOs.Model.Enums.EntityEnums.DeviceStatus;

namespace CabinetOs.Business.Concrete;

/// <summary>
/// Kamera izleme yolu — yoklama sonucunun yazildigi yer.
///
/// <b>Bu turda yoklamayi YAPAN bir servis yoktur</b> (kullanici karari: ping
/// gorevini kendisi yazacak). Burasi o gorevin yazacagi hedeftir ve gorevin
/// in-process mi yoksa harici mi oldugundan bagimsizdir.
/// </summary>
public partial class CameraService
{
    public async Task<Result> RecordProbeResultAsync(Guid cameraId, CameraProbeResultDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures, description: "Validation failed for CameraProbeResultDto");

        var camera = await _unitOfWork.Cameras.GetAsync(
            where: c => c.Id == cameraId,
            tracking: true,
            cancellationToken: cancellationToken);

        if (camera == null)
            return Result.NotFound(description: "Kamera bulunamadi");

        var nextStatus = request.Reachable ? (int)DeviceStatusEnum.Online : (int)DeviceStatusEnum.Offline;
        var nextError = request.Reachable ? null : Truncate(request.Error, 512);

        // DEGISMEDIYSE HIC YAZMA. Ingest'in "degeri degismeyen kanala dokunma"
        // kuralinin aynisi ve ayni gerekcesi: 5 dakikada bir yoklanan, surekli
        // ayakta bir kamera aksi halde gunde 288 anlamsiz UPDATE uretirdi.
        //
        // LastSeen bu kuralin DISINDA tutuluyor ve basarili her yoklamada
        // tazeleniyor: "en son ne zaman ayaktaydi" sorusunun tek kaynagi o, ve
        // yalnizca durum degisiminde yazilsaydi bir kamera aylarca Online kalip
        // LastSeen'i ilk yoklama aninda donmus gorunurdu.
        bool statusChanged = camera.DeviceStatusId != nextStatus;
        bool errorChanged = camera.LastConnectionError != nextError;

        if (request.Reachable)
            camera.LastSeen = DateTime.UtcNow;

        if (!statusChanged && !errorChanged && !request.Reachable)
            return Result.Success();

        camera.DeviceStatusId = nextStatus;
        camera.LastConnectionError = nextError;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static string? Truncate(string? value, int max)
        => value == null || value.Length <= max ? value : value[..max];
}
