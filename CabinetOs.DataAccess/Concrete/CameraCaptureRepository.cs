using Microsoft.EntityFrameworkCore;
using CabinetOs.DataAccess.Abstract;
using CabinetOs.DataAccess.Contexts;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Concrete
{
    public class CameraCaptureRepository : RepositoryBase<CameraCapture, AppDbContext>, ICameraCaptureRepository
    {
        public CameraCaptureRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ICollection<CameraCapture>> GetRecentForCameraAsync(Guid cameraId, int take, CancellationToken cancellationToken = default)
        {
            // Siralama CapturedAtUtc'ye gore — satirin YAZILDIGI an degil,
            // goruntunun ANI. Ikisi ayrisabilir: bir klip olay oncesini de
            // kapsadigi icin cekim isteginden ONCEKI bir ani tasiyabilir.
            // Esitlikte Id kirilir ki sira kararli olsun.
            return await _context.CameraCaptures
                .AsNoTracking()
                .Where(p => p.CameraId == cameraId)
                .OrderByDescending(p => p.CapturedAtUtc)
                .ThenByDescending(p => p.Id)
                .Take(take)
                .ToListAsync(cancellationToken);
        }
    }
}
