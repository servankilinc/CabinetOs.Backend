using CabinetOs.Model.Enums;

namespace CabinetOs.Model.Dtos.Scada.Commands;

/// <summary>
/// Kartin nokta dili — cerceve basligi ('I'/'A'/'O') ile bizim
/// <see cref="EntityEnums.PinDirection"/>'imiz arasindaki TEK ceviri yeri.
///
/// Iki yonu vardir ve ikisi ASIMETRIKTIR:
/// <list type="bullet">
/// <item><b>Gelen (telemetri).</b> Govde cerceveyi alanlarina bolerek tasir:
/// <c>type: "I" | "A"</c> + <c>channelNumber</c>. Basligi yone ceviren
/// <see cref="TryParseType"/>'dir.</item>
/// <item><b>Giden (kumanda).</b> Zarf noktayi TEK metin olarak tasir —
/// <c>"OUT5"</c>, <c>"OUT17"</c> (LED). Uretimi <see cref="Format"/>'ta.</item>
/// </list>
///
/// <b>Tek ceviri yeri olmasi bilincli.</b> Hem dogrulama hem servis buradan
/// gecer; ikisi ayri ayri cozseydi "dogrulamadan gecti ama cozulemedi" gibi
/// sessiz bir tutarsizlik mumkun olurdu.
///
/// Sozlesme: <c>docs/api-contract/07-scada-ingest.md</c>
/// </summary>
public static class ScadaPinAddress
{
    /// <summary>
    /// Cerceve basligi -> yon. <c>"I"</c> dijital giris, <c>"A"</c> analog giris.
    /// Buyuk/kucuk harf duyarsiz, bastaki/sondaki bosluk tolere edilir.
    ///
    /// <b><c>"O"</c> KASTEN taninmaz.</b> Cikis telemetri gondermez: bir roleyi biz
    /// surdugumuzde donen deger saha olayi degil kendi komutumuzun yankisidir ve
    /// kaydi zaten <c>DeviceCommand</c>'dadir. Referans projedeki cozumleyici de
    /// gelen tarafta yalnizca <c>'I'</c> ve <c>'A'</c> isler. Boylece gecersiz bir
    /// yon govdede IFADE EDILEMEZ hale gelir — ayrica reddedilmesi gereken bir
    /// durum olmaktan cikar.
    ///
    /// <c>Bidirectional</c>'in de karsiligi yoktur: kart her noktayi ya giris ya
    /// cikis olarak adresler.
    /// </summary>
    public static bool TryParseType(string? type, out EntityEnums.PinDirection direction)
    {
        direction = default;
        if (string.IsNullOrWhiteSpace(type)) return false;

        switch (type.Trim().ToUpperInvariant())
        {
            case "I":
                direction = EntityEnums.PinDirection.Input;
                return true;
            case "A":
                direction = EntityEnums.PinDirection.AnalogInput;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Kanal referansindan metinsel adres.
    ///
    /// Iki yerde kullanilir: giden kumanda zarfinin <c>Pin</c> alani ve INSAN OKUR
    /// metinler (diyagram kaydetme hatalari, ingest'in "tanimsiz pin" uyarisi).
    /// Ikisinde de kartin kendi dili, <c>direction: 0, channelNumber: 1</c>
    /// ikilisinden karsilastirilamayacak kadar okunaklidir.
    /// </summary>
    public static string Format(EntityEnums.PinDirection direction, int channelNumber) =>
        direction switch
        {
            EntityEnums.PinDirection.Input => $"IN{channelNumber}",
            EntityEnums.PinDirection.AnalogInput => $"AI{channelNumber}",
            EntityEnums.PinDirection.Output => $"OUT{channelNumber}",
            // Bidirectional'in kart karsiligi yok: kart her noktayi ya giris ya
            // cikis olarak adresler. Kumanda yolu bu kanali cikis olarak surer,
            // adres de oyle yazilir.
            _ => $"OUT{channelNumber}"
        };
}
