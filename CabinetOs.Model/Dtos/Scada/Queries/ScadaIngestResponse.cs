using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Scada.Queries;

/// <summary>
/// <c>POST /api/Scada/ingest</c> yaniti.
///
/// Neden bir sayim donuyor: tanimadigi cihaz kodunu ve kanal numarasini ingest
/// SESSIZCE ATLAR, tum istegi reddetmez — sahada bir modul eklendiginde o kabinin
/// TUM telemetrisi durmamali. Ama sessiz atlama tesbit edilemezse yanlis
/// yapilandirilmis bir SCADA aylarca "200 OK" alip hicbir sey yazmaz. Bu sayac,
/// sessizligi gorunur kilan tek sey.
///
/// Sozlesme: <c>docs/api-contract/07-scada-ingest.md</c>
/// </summary>
public class ScadaIngestResponse : IDto
{
    /// <summary>Cozumlenip islenen kanal okumasi sayisi.</summary>
    public int Accepted { get; set; }

    /// <summary>
    /// Kabul edilenlerin icinden GERCEKTEN degisenler. Degeri ayni kalan kanal
    /// yazilmaz ve yayin uretmez; <c>accepted &gt; 0</c> ama <c>changed = 0</c>
    /// saglikli bir durumdur (saha sabit).
    /// </summary>
    public int Changed { get; set; }

    /// <summary>Cozumlenemeyen cihaz + kanal referansi sayisi.</summary>
    public int Skipped { get; set; }

    /// <summary>
    /// Atlananlarin okunabilir referanslari (<c>"MOD-09"</c>, <c>"MOD-01/ch:42"</c>).
    /// Govdenin sinirsiz buyumemesi icin <see cref="MaxSkippedRefs"/> ile kirpilir;
    /// <see cref="Skipped"/> her zaman TAM sayiyi tasir.
    /// </summary>
    public List<string> SkippedRefs { get; set; } = [];

    /// <summary>Sunucunun islemi tamamladigi an — SCADA'nin gonderdigi zaman degil.</summary>
    public DateTime ReceivedAtUtc { get; set; }

    public const int MaxSkippedRefs = 50;
}
