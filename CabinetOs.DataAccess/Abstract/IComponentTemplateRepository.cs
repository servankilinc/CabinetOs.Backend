using System.Linq.Expressions;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Abstract
{
    public interface IComponentTemplateRepository : IRepository<ComponentTemplate>, IRepositoryAsync<ComponentTemplate>
    {
    }
}