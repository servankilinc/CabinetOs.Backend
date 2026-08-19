using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.ComponentTemplate.Queries
{
    public class ComponentTemplateBaseDto : IDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int DeviceTypeId { get; set; }
        public bool IsSystemTemplate { get; set; }
        public int BackgroundColor { get; set; }
        public string? BackgroundImageUrl { get; set; }
    }
}