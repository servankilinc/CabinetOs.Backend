using System.Text.Json.Serialization;
using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Diagram.Commands;

/// <summary> Tek bir entity gurubunun degisiklik kumesi. </summary>
/// <typeparam name="TCreate">Olusturma taslagi (<c>TempId</c> tasir, <c>Id</c> tasimaz).</typeparam>
/// <typeparam name="TUpdate">Guncelleme taslagi (<c>Id</c> tasir, <c>TempId</c> tasimaz).</typeparam>
public class EntityDelta<TCreate, TUpdate> : IDto
{
    public List<TCreate> Created { get; set; } = [];
    public List<TUpdate> Updated { get; set; } = [];
    public List<Guid> Deleted { get; set; } = [];

    /// <summary> Uc liste de bossa bu aile icin hicbir is yapilmaz. (db transaction açılmaz) </summary>
    [JsonIgnore]
    public bool IsEmpty => Created.Count == 0 && Updated.Count == 0 && Deleted.Count == 0;
}
