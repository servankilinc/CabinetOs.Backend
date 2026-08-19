using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Common;

public class SelectItemDto : IDto
{
    public string Value { get; set; } = null!;
    public string Text { get; set; } = null!;
}
