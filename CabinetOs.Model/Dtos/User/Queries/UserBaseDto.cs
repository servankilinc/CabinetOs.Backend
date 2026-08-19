using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.User.Queries
{
    public class UserBaseDto : IDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public Guid CompanyId { get; set; }
    }
}