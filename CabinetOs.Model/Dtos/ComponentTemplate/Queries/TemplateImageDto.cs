using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.ComponentTemplate.Queries;

/// <summary>Yuklenen ComponentTemplate gorselinin sonucu. </summary>
public class TemplateImageDto : IDto
{
    public string Url { get; set; } = null!;
}
