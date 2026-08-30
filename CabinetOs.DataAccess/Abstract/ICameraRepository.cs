using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Abstract
{
    public interface ICameraRepository : IRepository<Camera>, IRepositoryAsync<Camera>
    {
    }
}
