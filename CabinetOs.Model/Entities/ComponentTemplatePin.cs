using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Entities;

public class ComponentTemplatePin : IEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ComponentTemplateId { get; set; }
    public string Name { get; set; } = null!;
    public double RelativeX { get; set; }
    public double RelativeY { get; set; }
    public int? ChannelNumber { get; set; }
    public PinFunction Function { get; set; }
    public PinDirection Direction { get; set; }
    public SignalLayer SignalLayer { get; set; }
    public VoltageLevel? VoltageLevel { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? CreateDateUtc { get; set; }
    public DateTime? UpdateDateUtc { get; set; }
    public virtual ComponentTemplate? ComponentTemplate { get; set; }
}