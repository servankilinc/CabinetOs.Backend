using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using CabinetOs.Core.Enums;
using CabinetOs.Core.Utils;
using CabinetOs.Core.Utils.Auth;
using CabinetOs.Core.Utils.HttpContextManager;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Auth.Login;
using CabinetOs.Model.Auth.Logout;
using CabinetOs.Model.Auth.Refresh;
using CabinetOs.Model.Auth.SignUp;
using CabinetOs.Model.Entities;
using CabinetOs.Business.Abstract;
using CabinetOs.Business.Utils.TokenService;

namespace CabinetOs.Business.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpContextManager _httpContextManager;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, UserManager<User> userManager, SignInManager<User> signInManager, IHttpContextManager httpContextManager, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextManager = httpContextManager;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(loginRequest, cancellationToken);
            if (!validationResult.IsValid)
                return Result<LoginResponse>.Validation(validationResult.Failures);
            // 1) Find user by credentials
            User? user = null;
            if (loginRequest.Email != null)
            {
                user = await _userManager.FindByEmailAsync(loginRequest.Email);
            }
            else if (loginRequest.UserName != null)
            {
                user = await _userManager.FindByNameAsync(loginRequest.UserName);
            }

            // 2) Check CanSignInAsync and CheckPasswordSignInAsync (Rejected credentials are a client-side condition (403), not a server fault (500))
            if (user == null)
                return Result<LoginResponse>.Forbidden(message: "Credentials are incorrect.", metadata: GlobalExtensions.Meta("Requester Email or Username", loginRequest.Email ?? loginRequest.UserName));
            SignInResult checkPassword = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: true);
            if (!checkPassword.Succeeded)
            {
                if (checkPassword.IsLockedOut)
                    return Result<LoginResponse>.Forbidden(message: "Your account is temporarily locked due to multiple failed login attempts.", metadata: GlobalExtensions.Meta("Requester Email", loginRequest.Email));
                if (checkPassword.RequiresTwoFactor)
                    return Result<LoginResponse>.Forbidden(message: "Two-factor authentication is required to login.", metadata: GlobalExtensions.Meta("Requester Email", loginRequest.Email));
                if (checkPassword.IsNotAllowed)
                    return Result<LoginResponse>.Forbidden(message: "The user is not allowed to sign in.", metadata: GlobalExtensions.Meta("Requester Email", loginRequest.Email));
                return Result<LoginResponse>.Forbidden(message: "Credentials are incorrect.", metadata: GlobalExtensions.Meta("Requester Email or Username", loginRequest.Email ?? loginRequest.UserName));
            }

            if (!await _signInManager.CanSignInAsync(user))
            {
                return Result<LoginResponse>.Forbidden(message: "You are not allowed to login.", metadata: GlobalExtensions.Meta("User", user));
            }

            // 3) Get user roles and claims
            IList<string> roles = await _userManager.GetRolesAsync(user);
            IList<Claim> claims = await GetClaimsAsync(user, roles);
            // 3) Generate Access Token and Refresh Token
            Result<AccessToken> accessToken = _tokenService.GenerateAccessToken(claims);
            if (!accessToken.IsSuccess)
                return Result<LoginResponse>.Failure(description: "Access token could not generated", metadata: GlobalExtensions.Meta("Access Token Result", accessToken));
            string tokenValue = _tokenService.GenerateRandomNumber();
            Result<RefreshToken> refreshToken = _tokenService.GenerateRefreshToken(user, tokenValue, loginRequest.ClientType, loginRequest.DeviceId);
            if (!refreshToken.IsSuccess)
                return Result<LoginResponse>.Failure(description: "Refresh token could not generated", metadata: GlobalExtensions.Meta("Refresh Token Result", refreshToken));
            // 4) Save Refresh Token and Revoke old ones if deviceId is provided
            if (loginRequest.DeviceId != null && loginRequest.DeviceId.HasValue)
            {
                await _unitOfWork.RefreshTokens.RevokeDeviceRefreshTokensAsync(f => f.DeviceId == loginRequest.DeviceId.Value && f.IsRevoked == false);
            }

            await _unitOfWork.RefreshTokens.AddAndSaveAsync(refreshToken.Data, cancellationToken);
            if (refreshToken.Data.ClientType != ClientType.Web)
            {
                return Result<LoginResponse>.Success(new LoginTrustedResponse { AccessToken = accessToken.Data, RefreshToken = tokenValue, DeviceId = refreshToken.Data.DeviceId, //User = user,
                Roles = roles });
            }
            else
            {
                _httpContextManager.AddRefreshTokenToCookie(tokenValue, refreshToken.Data.ExpirationUtc);
                return Result<LoginResponse>.Success(new LoginResponse { AccessToken = accessToken.Data, DeviceId = refreshToken.Data.DeviceId, //User = user,
                Roles = roles });
            }
        }

        public async Task<Result<SignUpResponse>> SignUpAsync(SignUpRequest signUpRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                var validationResult = await _validationService.ValidateAsync(signUpRequest, cancellationToken);
                if (!validationResult.IsValid)
                    return Result<SignUpResponse>.Validation(validationResult.Failures);
                // 1) Check if user already exists
                var userExist = await _userManager.FindByEmailAsync(signUpRequest.Email);
                if (userExist != null)
                    return Result<SignUpResponse>.Validation(new Dictionary<string, string[]> { [nameof(signUpRequest.Email)] = new[] { "The email address is already in use." } }, message: "The email address is already in use.");
                userExist = await _userManager.FindByNameAsync(signUpRequest.UserName);
                if (userExist != null)
                    return Result<SignUpResponse>.Validation(new Dictionary<string, string[]> { [nameof(signUpRequest.UserName)] = new[] { "The user name is already in use." } }, message: "The user name is already in use.");
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                // 2) Create new user
                var user = _mapper.Map<User>(signUpRequest);
                var result = await _userManager.CreateAsync(user, signUpRequest.Password);
                if (!result.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<SignUpResponse>.Failure(description: $"User cannot be created.", metadata: GlobalExtensions.Meta(("Requester Email", signUpRequest.Email), ("Identity Service Errors", result)));
                }

                // 3) Assign "User" role to the new user
                var roleResult = await _userManager.AddToRoleAsync(user, RoleType.User.GetDescription());
                if (!roleResult.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<SignUpResponse>.Failure(description: $"Failed to assign role", metadata: GlobalExtensions.Meta(("Requester Email", signUpRequest.Email), ("Identity Service Errors", roleResult)));
                }

                // 4) Get user roles and claims
                IList<string> roles = await _userManager.GetRolesAsync(user);
                IList<Claim> claims = await GetClaimsAsync(user, roles);
                // 5) Generate Access Token and Refresh Token
                Result<AccessToken> accessToken = _tokenService.GenerateAccessToken(claims);
                if (!accessToken.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<SignUpResponse>.Failure(description: "Access token could not generated", metadata: GlobalExtensions.Meta("Access Token Result", accessToken));
                }

                string tokenValue = _tokenService.GenerateRandomNumber();
                Result<RefreshToken> refreshToken = _tokenService.GenerateRefreshToken(user, tokenValue, signUpRequest.ClientType, signUpRequest.DeviceId);
                if (!refreshToken.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<SignUpResponse>.Failure(description: "Refresh token could not generated", metadata: GlobalExtensions.Meta("Refresh Token Result", refreshToken));
                }

                // 6) Save Refresh Token and Revoke old ones if deviceId is provided
                if (signUpRequest.DeviceId != null && signUpRequest.DeviceId.HasValue)
                {
                    await _unitOfWork.RefreshTokens.RevokeDeviceRefreshTokensAsync(f => f.DeviceId == signUpRequest.DeviceId.Value && f.IsRevoked == false);
                }

                await _unitOfWork.RefreshTokens.AddAndSaveAsync(refreshToken.Data, cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                if (signUpRequest.ClientType != ClientType.Web)
                {
                    return Result<SignUpResponse>.Success(new SignUpTrustedResponse { AccessToken = accessToken.Data, RefreshToken = tokenValue, DeviceId = refreshToken.Data.DeviceId, //User = user,
                    Roles = roles, });
                }
                else
                {
                    _httpContextManager.AddRefreshTokenToCookie(tokenValue, refreshToken.Data.ExpirationUtc);
                    return Result<SignUpResponse>.Success(new SignUpResponse { AccessToken = accessToken.Data, DeviceId = refreshToken.Data.DeviceId, //User = user,
                    Roles = roles, });
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<Result<RefreshAuthResponse>> RefreshAsync(RefreshAuthRequest refreshAuthRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                var validationResult = await _validationService.ValidateAsync(refreshAuthRequest, cancellationToken);
                if (!validationResult.IsValid)
                    return Result<RefreshAuthResponse>.Validation(validationResult.Failures);
                // 1) Set refresh token from cookie if not provided
                if (string.IsNullOrWhiteSpace(refreshAuthRequest.RefreshToken))
                {
                    var cookieValue = _httpContextManager.GetRefreshTokenFromCookie();
                    if (!cookieValue.IsSuccess)
                        return Result<RefreshAuthResponse>.Forbidden(message: "Your session has expired. Please sign in again.", description: "Refresh auth request cookie not found in cookie", metadata: GlobalExtensions.Meta("Cookie Result", cookieValue.Error.Description));
                    refreshAuthRequest.RefreshToken = cookieValue.Data;
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                string hashedToken = _tokenService.HashToken(refreshAuthRequest.RefreshToken);
                // 2) Find refresh token record
                RefreshToken? refreshToken = await _unitOfWork.RefreshTokens.GetAsync(where: f => f.UserId == refreshAuthRequest.UserId && f.DeviceId == refreshAuthRequest.DeviceId && f.Token == hashedToken && f.TTL > 0 && f.IsRevoked == false && f.ExpirationUtc > DateTime.UtcNow, cancellationToken: cancellationToken);
                if (refreshToken == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<RefreshAuthResponse>.Forbidden(message: "Your session has expired. Please sign in again.", description: "There is no refresh token that can be used.", metadata: GlobalExtensions.Meta("Request Model", refreshAuthRequest));
                }

                // 3) Find user
                var user = await _unitOfWork.Users.GetAsync(where: f => f.Id == refreshAuthRequest.UserId, cancellationToken: cancellationToken);
                if (user == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<RefreshAuthResponse>.Forbidden(message: "Your session has expired. Please sign in again.", description: $"User cannot found for refresh auth, userId: {refreshAuthRequest.UserId}", metadata: GlobalExtensions.Meta("Request Model", refreshAuthRequest));
                }

                // 4) Update refresh token 
                string tokenValue = _tokenService.GenerateRandomNumber();
                refreshToken.Token = _tokenService.HashToken(tokenValue);
                refreshToken.TTL -= 1;
                if (refreshToken.TTL <= 0)
                    refreshToken.IsRevoked = true;
                await _unitOfWork.RefreshTokens.UpdateAndSaveAsync(refreshToken, cancellationToken);
                // 5) revoke old tokens for the device
                await _unitOfWork.RefreshTokens.RevokeDeviceRefreshTokensAsync(f => f.DeviceId == refreshAuthRequest.DeviceId && f.IsRevoked == false && f.Id != refreshToken.Id, cancellationToken);
                // 6) Get user roles and claims
                IList<string> roles = await _userManager.GetRolesAsync(user);
                IList<Claim> claims = await GetClaimsAsync(user, roles);
                // 7) Generate new access token
                Result<AccessToken> accessToken = _tokenService.GenerateAccessToken(claims);
                if (!accessToken.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<RefreshAuthResponse>.Failure(description: "Access token could not generated", metadata: GlobalExtensions.Meta("Access Token Result", accessToken));
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                if (refreshToken.ClientType != ClientType.Web)
                {
                    return Result<RefreshAuthResponse>.Success(new RefreshAuthTrustedResponse { AccessToken = accessToken.Data, RefreshToken = tokenValue, Roles = roles });
                }
                else
                {
                    _httpContextManager.AddRefreshTokenToCookie(tokenValue, refreshToken.ExpirationUtc);
                    return Result<RefreshAuthResponse>.Success(new RefreshAuthResponse { AccessToken = accessToken.Data, Roles = roles });
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<Result> LogoutAsync(LogoutRequest logoutRequest, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(logoutRequest, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures);
            // Fall back to the cookie when the caller did not send the token explicitly (web clients).
            if (string.IsNullOrWhiteSpace(logoutRequest.RefreshToken))
            {
                var cookieValue = _httpContextManager.GetRefreshTokenFromCookie();
                if (cookieValue.IsSuccess)
                    logoutRequest.RefreshToken = cookieValue.Data;
            }

            if (!string.IsNullOrWhiteSpace(logoutRequest.RefreshToken))
            {
                string hashedToken = _tokenService.HashToken(logoutRequest.RefreshToken);
                await _unitOfWork.RefreshTokens.RevokeDeviceRefreshTokensAsync(f => f.UserId == logoutRequest.UserId && f.DeviceId == logoutRequest.DeviceId && f.Token == hashedToken && f.IsRevoked == false, cancellationToken);
            }
            else
            {
                // No token supplied: revoke everything issued to this device.
                await _unitOfWork.RefreshTokens.RevokeDeviceRefreshTokensAsync(f => f.UserId == logoutRequest.UserId && f.DeviceId == logoutRequest.DeviceId && f.IsRevoked == false, cancellationToken);
            }

            _httpContextManager.DeletetRefreshTokenFromCookie();
            return Result.Success();
        }

        public async Task<Result> RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == default)
                return Result.Validation(new Dictionary<string, string[]> { ["userId"] = new[] { "A valid user id is required." } }, message: "A valid user id is required.");
            await _unitOfWork.RefreshTokens.RevokeDeviceRefreshTokensAsync(f => f.UserId == userId && f.IsRevoked == false, cancellationToken);
            // Rotating the security stamp invalidates cookies and any stamp-bound tokens.
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
                await _userManager.UpdateSecurityStampAsync(user);
            _httpContextManager.DeletetRefreshTokenFromCookie();
            return Result.Success();
        }

        private async Task<IList<Claim>> GetClaimsAsync(User user, IList<string>? roles = default)
        {
            string displayName = $"{user.Name} {user.SurName}".Trim();
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = user.UserName ?? user.Email ?? user.Id.ToString();
            List<Claim> claimList = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, displayName)
            };
            if (!string.IsNullOrEmpty(user.Email))
                claimList.Add(new Claim(ClaimTypes.Email, user.Email));
            IList<Claim>? persistentClaims = await _userManager.GetClaimsAsync(user);
            claimList.AddRange(persistentClaims);
            IEnumerable<Claim> roleClaims = (roles ?? Array.Empty<string>()).Select(role => new Claim(ClaimTypes.Role, role));
            claimList.AddRange(roleClaims);
            // password, role vs. deÄŸiÅŸdiÄŸinde mevcut tokenlarÄ± geÃ§ersiz kÄ±lmak iÃ§in security stamp eklenebilir
            // var securityStamp = await _userManager.GetSecurityStampAsync(user);
            // claimList.Add(new Claim("app_security_stamp_claim", securityStamp));
            return claimList;
        }
    }
}