using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Abstract
{
    public interface ICameraCaptureRepository : IRepository<CameraCapture>, IRepositoryAsync<CameraCapture>
    {
        /// <summary>
        /// Bir kameranin son cekimleri, yeniden eskiye.
        /// </summary>
        /// <remarks>
        /// <c>IDeviceCommandRepository.GetRecentForDeviceAsync</c> ile ayni gerekce:
        /// jenerik <c>GetAllAsync</c> imzasinda <c>take</c> yok ve sinirsiz cekip
        /// bellekte kirpmak, cekim gecmisi buyudukce tum tabloyu okumak demek.
        /// </remarks>
        Task<ICollection<CameraCapture>> GetRecentForCameraAsync(Guid cameraId, int take, CancellationToken cancellationToken = default);
    }
}
