using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.Diagram.Queries.Items;

public class DiagramCanvasSettingsDto : IDto
{
    public int GridSize { get; set; }
    public bool SnapToGrid { get; set; }
    public BackgroundVariant BackgroundVariant { get; set; }
    public string GridColor { get; set; } = null!;
    public string BackgroundColor { get; set; } = null!;
    public double MinZoom { get; set; }
    public double MaxZoom { get; set; }
}
