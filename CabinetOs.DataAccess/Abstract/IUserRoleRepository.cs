using System.Linq.Expressions;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Abstract
{
    public interface IUserRoleRepository : IRepository<UserRole>, IRepositoryAsync<UserRole>
    {
    }
}