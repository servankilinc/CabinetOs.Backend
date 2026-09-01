using System.Text.Json.Serialization;
using CabinetOs.Core.Model;
using CabinetOs.Model.Dtos.Diagram.Commands.Draft.Abstract;

namespace CabinetOs.Model.Dtos.Diagram.Commands;

/// <summary>
/// Tek bir entity ailesinin degisiklik kumesi.
///
/// <b><c>created</c> / <c>updated</c> ayrimi YOKTUR.</b> Guid'i istemci uretir,
/// dolayisiyla bir taslagin yeni mi yoksa mevcut mu oldugu tek bir yerde —
/// Id'nin veritabaninda bulunup bulunmamasinda — cevaplanir. Istemcinin "bu kayit
/// sunucuya gitti mi" bilgisini tasimasi gerekmez.
/// </summary>
/// <typeparam name="T">Ailenin taslagi; her zaman <c>Id</c> tasir.</typeparam>
public class EntityDelta<T> : IDto where T : IIdentifiableDraft
{
    public List<T> Upserted { get; set; } = [];
    public List<Guid> Deleted { get; set; } = [];

    /// <summary> Iki liste de bossa bu aile icin hicbir is yapilmaz. </summary>
    [JsonIgnore]
    public bool IsEmpty => Upserted.Count == 0 && Deleted.Count == 0;
}
