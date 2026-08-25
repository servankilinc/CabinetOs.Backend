using CabinetOs.Core.Enums;
using CabinetOs.Core.Utils.ResultPattern;

namespace CabinetOs.Core.Utils.HttpContextManager
{
    public interface IHttpContextManager
    {
        Result<string> GetNameIdentifier();
        /// <summary> Oturumdaki kullanicinin GORUNEN adi (<c>ClaimTypes.Name</c>). </summary>
        Result<string> GetName();
        Result<string> GetUserAgent();
        Result<string> GetClientIp();
        Result<string> GetCurrentCulture();
        Result<byte> GetCurrentLanguageId();
        Result<Language> GetCurrentLanguage();
        Result SetCurrentCulture(string culture);
        Result<string> GetRefreshTokenFromCookie();
        Result AddRefreshTokenToCookie(string refreshToken, DateTime expirationUtc);
        Result DeletetRefreshTokenFromCookie();
    }
}