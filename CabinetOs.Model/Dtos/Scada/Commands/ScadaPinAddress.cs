using System.Globalization;
using CabinetOs.Model.Enums;

namespace CabinetOs.Model.Dtos.Scada.Commands;

/// <summary>
/// Kart uzerindeki bir noktanin metinsel adresi — <c>"IN1"</c>, <c>"OUT17"</c>.
///
/// <b>Neden tek string, neden iki ayri alan degil.</b> Bu, kartin kendi dilidir:
/// 5 baytlik cerceve noktayi bir yon basligi ('I'/'O') ve bir numara ile
/// adresler, referans proje de ayni ikiliyi <c>InOut + Code</c> olarak saklar.
/// Govdede <c>"IN1"</c> gormek, SCADA ekibine verilen spesifikasyonda
/// <c>direction: 0, channelNumber: 1</c>'den karsilastirilamayacak kadar
/// okunaklidir.
///
/// <b>Tek ayristirici olmasi bilincli.</b> Hem dogrulama hem servis buradan
/// gecer; ikisi ayri ayri ayristirsaydi "dogrulamadan gecti ama cozulemedi"
/// gibi sessiz bir tutarsizlik mumkun olurdu.
///
/// Sozlesme: <c>docs/api-contract/07-scada-ingest.md</c>
/// </summary>
public readonly record struct ScadaPinAddress(EntityEnums.PinDirection Direction, int ChannelNumber)
{
    /// <summary>
    /// Numaranin en fazla basamak sayisi. Sinir keyfi degil: <c>int.Parse</c>'in
    /// tasmasini ayristirmadan ONCE imkansiz kilar, boylece gecersiz girdi
    /// istisna degil <c>false</c> uretir.
    /// </summary>
    private const int MaxDigits = 4;

    /// <summary>
    /// <c>"IN"</c> / <c>"OUT"</c> on eki -> yon. Buyuk/kucuk harf duyarsiz.
    ///
    /// LED icin AYRI bir on ek YOK ve bu bilincli: kart LED'i rolelerle ayni duz
    /// cikis uzayinda adresliyor (role 1-16, LED 17-24) ve ikisine de ayni 'O'
    /// basligini yaziyor. LED = <c>OUT17..OUT24</c>.
    /// </summary>
    private static readonly (string Prefix, EntityEnums.PinDirection Direction)[] Prefixes =
    [
        // "OUT" once denenmeli: "IN" ile baslamiyor ama uzun on ekin kisa olandan
        // once gelmesi, ileride ortak harfle baslayan bir on ek eklenirse
        // kuralin kendiliginden dogru kalmasini saglar.
        ("OUT", EntityEnums.PinDirection.Output),
        ("IN", EntityEnums.PinDirection.Input)
    ];

    public static bool TryParse(string? value, out ScadaPinAddress address)
    {
        address = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var text = value.Trim();

        foreach (var (prefix, direction) in Prefixes)
        {
            if (!text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

            var digits = text.AsSpan(prefix.Length);
            if (digits.Length is 0 or > MaxDigits) return false;

            foreach (var c in digits)
                if (!char.IsAsciiDigit(c)) return false;

            var number = int.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture);

            // Kartta 0 diye bir nokta yok; "IN0" bir yazim hatasidir, gecerli bir
            // adres degil.
            if (number <= 0) return false;

            address = new ScadaPinAddress(direction, number);
            return true;
        }

        return false;
    }

    /// <summary>Kanal referansindan metinsel adres — giden kumanda govdesi icin.</summary>
    public static string Format(EntityEnums.PinDirection direction, int channelNumber) =>
        direction switch
        {
            EntityEnums.PinDirection.Output => $"OUT{channelNumber}",
            EntityEnums.PinDirection.Input => $"IN{channelNumber}",
            // Bidirectional'in kart karsiligi yok: kart her noktayi ya giris ya
            // cikis olarak adresler. Kumanda yolu bu kanali cikis olarak surer,
            // adres de oyle yazilir.
            _ => $"OUT{channelNumber}"
        };

    public override string ToString() => Format(Direction, ChannelNumber);
}
