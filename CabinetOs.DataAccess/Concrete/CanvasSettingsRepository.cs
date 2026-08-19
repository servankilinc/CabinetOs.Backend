using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CabinetOs.DataAccess.Abstract;
using CabinetOs.DataAccess.Contexts;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Concrete
{
    public class CanvasSettingsRepository : RepositoryBase<CanvasSettings, AppDbContext>, ICanvasSettingsRepository
    {
        public CanvasSettingsRepository(AppDbContext context) : base(context)
        {
        }
    }
}