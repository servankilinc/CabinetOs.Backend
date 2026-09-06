using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Business.Utils;

/// <summary>
/// "Yuku ver / yuku kes" niyetini telde gidecek <c>"1"</c>/<c>"0"</c>'a cevirir.
///
/// <b>Neden bir cevirme gerekiyor.</b> Karta giden bayt rolenin BOBININI surer.
/// Bobin enerjilendiginde yukun devresinin kapanip kapanmadigi, yukun hangi
/// kontaga kablolandigina baglidir:
///
/// <list type="bullet">
/// <item><b>NO</b> (Normally Open): bobin enerjisizken kontak acik. Yuku vermek
/// icin bobin enerjilenir -> <c>"1"</c>.</item>
/// <item><b>NC</b> (Normally Closed): bobin enerjisizken kontak kapali. Yuku
/// vermek icin bobin enerjisiz BIRAKILIR -> <c>"0"</c>.</item>
/// </list>
///
/// Yani ayni "ac" niyeti, kablolamaya gore ters bayt uretir. Bu bilgi
/// <c>Pin.Function</c>'da duruyor ve karari sunucuda vermek, operatorun her
/// role icin bunu ezberlemesinden cok daha guvenli.
/// </summary>
public static class ContactPolarity
{
    /// <summary>
    /// <paramref name="contact"/> <c>null</c> ise kutup sorusu YOKTUR ve dogrudan
    /// eslesme kullanilir. Bu, LED'ler ve duz dijital cikislar icin gecerli:
    /// onlarda NO/NC kontagi diye bir sey yok, "ac" dogrudan <c>"1"</c>'dir.
    /// </summary>
    public static string ToWireValue(bool turnOn, PinFunction? contact)
    {
        bool energize = contact == PinFunction.NC ? !turnOn : turnOn;
        return energize ? "1" : "0";
    }
}
