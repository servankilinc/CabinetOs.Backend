using System.Linq.Expressions;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Abstract
{
    public interface IDeviceCommandRepository : IRepository<DeviceCommand>, IRepositoryAsync<DeviceCommand>
    {
        /// <summary>
        /// Bir cihazin en son kumandalari, yeniden eskiye.
        /// </summary>
        /// <remarks>
        /// Jenerik <c>GetAllAsync</c> ile yazilamiyor: o imzada <c>take</c> yok ve
        /// <c>orderBy</c> parametresi <c>IOrderedQueryable</c> dondurmek zorunda
        /// oldugu icin icine <c>Take</c> sikistirilamaz. Sinirsiz cekip bellekte
        /// kirpmak ise kumanda gecmisi buyudukce tum tabloyu okumak demekti.
        /// </remarks>
        Task<ICollection<DeviceCommand>> GetRecentForDeviceAsync(Guid deviceId, int take, CancellationToken cancellationToken = default);
    }
}
