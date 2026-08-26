using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Diagram.Queries;

/// <summary>
/// <c>POST /api/Diagram/cabinet/{cabinetId}/save</c> yaniti.
///
/// Istemcinin bunu almadan yapamayacagi TEK sey gecici kimliklerin karsiligidir:
/// <c>tmp_*</c> ile olusturulan node ve edge'ler, sunucu Id'leriyle yeniden
/// yazilmadan bir sonraki kaydetmede yeniden OLUSTURULMUS olurdu.
/// </summary>
public class DiagramSaveResponse : IDto
{
    public List<IdMapEntry> Devices { get; set; } = [];
    public List<IdMapEntry> Connections { get; set; } = [];
    public List<IdMapEntry> Annotations { get; set; } = [];

    /// <summary>
    /// Sablondan uretilen pin sayisi. Istemci bu sayi sifirdan buyukse grafi
    /// yeniden cekmesi gerektigini bilir: o pinlerin gecici kimligi yoktur,
    /// dolayisiyla idMap ile ogrenilemezler.
    /// </summary>
    public int InstantiatedPinCount { get; set; }

    public DateTime SavedAtUtc { get; set; }
}
