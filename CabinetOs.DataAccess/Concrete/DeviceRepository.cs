using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CabinetOs.DataAccess.Abstract;
using CabinetOs.DataAccess.Contexts;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Concrete
{
    public class DeviceRepository : RepositoryBase<Device, AppDbContext>, IDeviceRepository
    {
        public DeviceRepository(AppDbContext context) : base(context)
        {
        }
    }
}