using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Diagram.Queries;

/// <summary>
/// <c>POST /api/Diagram/cabinet/{cabinetId}/save</c> yaniti.
///
/// Kimlik haritasi TASIMAZ: Guid'leri istemci urettigi icin geri ogrenecegi bir
/// sey yok. Yanittaki iki sayac yalnizca BILGILENDIRME amaclidir; istemcinin
/// dogru calismak icin ikisine de ihtiyaci yoktur.
/// </summary>
public class DiagramSaveResponse : IDto
{
    /// <summary>
    /// Sablondan uretilen pin sayisi. Pinleri hala sunucu uretir; istemci bu sayi
    /// sifirdan buyukse yeni cihazlarin pinlerini almak icin grafi tazeler.
    /// </summary>
    public int InstantiatedPinCount { get; set; }

    /// <summary>
    /// Karsiligi bulunamadigi icin ATLANAN silme sayisi.
    ///
    /// Bilinmeyen bir Id tum gonderiyi 400'e dusurmez — bu, istemcinin "bu kayit
    /// sunucuya gitti mi" bilgisini tasima zorunlulugunu kaldiran karardir
    /// (SCADA ingest'in tanimadigi kanali atlamasiyla ayni gerekce, K7).
    /// </summary>
    public int SkippedDeleteCount { get; set; }

    public DateTime SavedAtUtc { get; set; }
}
