using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.ComponentTemplate.Queries
{
    public class ComponentTemplateDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int DeviceTypeId { get; set; }
        public bool IsSystemTemplate { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int BackgroundColor { get; set; }
        public string? BackgroundImageUrl { get; set; }
        public string DeviceTypeName { get; set; } = null!;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public bool IsActive { get; set; }
    }
}