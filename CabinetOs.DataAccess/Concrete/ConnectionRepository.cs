using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CabinetOs.DataAccess.Abstract;
using CabinetOs.DataAccess.Contexts;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Concrete
{
    public class ConnectionRepository : RepositoryBase<Connection, AppDbContext>, IConnectionRepository
    {
        public ConnectionRepository(AppDbContext context) : base(context)
        {
        }
    }
}