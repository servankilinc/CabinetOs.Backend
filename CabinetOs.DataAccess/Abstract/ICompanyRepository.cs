using System.Linq.Expressions;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Abstract
{
    public interface ICompanyRepository : IRepository<Company>, IRepositoryAsync<Company>
    {
    }
}