using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Diagram.Queries;

/// <summary>
/// <c>POST /api/Diagram/cabinet/{cabinetId}/save</c> yaniti.
///
/// <b>Istemcinin geri OGRENECEGI hicbir sey yok.</b> Diyagramdaki her satirin —
/// cihaz, kablo, not, pin ve kanal dahil — Guid'ini istemci uretiyor, dolayisiyla
/// ne kimlik haritasi ne de "sunucu sunu da yaratti" bilgisi gerekiyor. Kaydetme
/// atomik oldugu icin 200 tek basina "gonderdigim her sey kalici" demektir.
///
/// Onceki iki sayac (<c>InstantiatedPinCount</c>, <c>SkippedDeleteCount</c>)
/// KALDIRILDI: ilki pinleri sunucu urettigi donemde grafi tazelemenin tetikleyicisiydi
/// ve o ihtiyac ortadan kalkti; ikincisini hicbir istemci okumuyordu. Atlanan silme
/// davranisi aynen duruyor (K7), yalnizca sayilmiyor.
/// </summary>
public class DiagramSaveResponse : IDto
{
    public DateTime SavedAtUtc { get; set; }
}
