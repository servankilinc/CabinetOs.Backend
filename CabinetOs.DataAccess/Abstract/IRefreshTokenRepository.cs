using System.Linq.Expressions;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Abstract
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>, IRepositoryAsync<RefreshToken>
    {
        void RevokeDeviceRefreshTokens(Expression<Func<RefreshToken, bool>> where);
        Task RevokeDeviceRefreshTokensAsync(Expression<Func<RefreshToken, bool>> where, CancellationToken cancellationToken = default);
    }
}