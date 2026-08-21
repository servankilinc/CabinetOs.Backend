using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Auth.Login;
using CabinetOs.Model.Auth.Logout;
using CabinetOs.Model.Auth.Refresh;
using CabinetOs.Model.Auth.SignUp;
using CabinetOs.Model.Dtos.User.Queries;

namespace CabinetOs.Business.Abstract
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default);
        Task<Result<SignUpResponse>> SignUpAsync(SignUpRequest signUpRequest, CancellationToken cancellationToken = default);
        Task<Result<RefreshAuthResponse>> RefreshAsync(RefreshAuthRequest refreshAuthRequest, CancellationToken cancellationToken = default);
        Task<Result> LogoutAsync(LogoutRequest logoutRequest, CancellationToken cancellationToken = default);
        Task<Result> RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Result<CurrentUserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}