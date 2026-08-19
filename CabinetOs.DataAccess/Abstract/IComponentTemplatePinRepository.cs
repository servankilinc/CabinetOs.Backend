using System.Linq.Expressions;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Abstract
{
    public interface IComponentTemplatePinRepository : IRepository<ComponentTemplatePin>, IRepositoryAsync<ComponentTemplatePin>
    {
    }
}