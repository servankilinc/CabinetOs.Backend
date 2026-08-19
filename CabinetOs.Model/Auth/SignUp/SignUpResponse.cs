using CabinetOs.Core.Utils.Auth;

namespace CabinetOs.Model.Auth.SignUp
{
    public class SignUpResponse
    {
        public IList<string>? Roles { get; set; }
        public AccessToken AccessToken { get; set; } = null!;
        public Guid DeviceId { get; set; }
    }

    public class SignUpTrustedResponse : SignUpResponse
    {
        public string RefreshToken { get; set; } = null!;
    }
}