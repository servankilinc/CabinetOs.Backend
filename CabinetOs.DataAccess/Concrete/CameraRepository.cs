using CabinetOs.DataAccess.Abstract;
using CabinetOs.DataAccess.Contexts;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Concrete
{
    public class CameraRepository : RepositoryBase<Camera, AppDbContext>, ICameraRepository
    {
        public CameraRepository(AppDbContext context) : base(context)
        {
        }
    }
}
