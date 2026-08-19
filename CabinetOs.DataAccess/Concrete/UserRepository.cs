using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CabinetOs.DataAccess.Abstract;
using CabinetOs.DataAccess.Contexts;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Concrete
{
    public class UserRepository : RepositoryBase<User, AppDbContext>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }
    }
}