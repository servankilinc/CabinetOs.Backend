using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Diagram.Queries;

/// <summary> Bir gecici kimligin karsiligi olan sunucu Id'si. </summary>
public class IdMapEntry : IDto
{
    public string TempId { get; set; } = null!;
    public Guid Id { get; set; }

    public IdMapEntry() { }

    public IdMapEntry(string tempId, Guid id)
    {
        TempId = tempId;
        Id = id;
    }
}
