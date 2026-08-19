using CabinetOs.Core.Utils.Auth;
using CabinetOs.Model.Dtos.User.Queries;

namespace CabinetOs.Model.Auth.Login;

public class LoginResponse
{
    public IList<string>? Roles { get; set; }
    public AccessToken AccessToken { get; set; } = null!;
    public Guid DeviceId { get; set; }
    public UserBaseDto User { get; set; } = null!;
}

public class LoginTrustedResponse : LoginResponse
{
    public string RefreshToken { get; set; } = null!;
}