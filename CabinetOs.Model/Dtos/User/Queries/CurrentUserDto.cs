using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.User.Queries;

/// <summary>
/// oturum sahibini access token'i kendisi cozmeden ogrenebilsin diye vardir.
/// </summary>
public class CurrentUserDto : IDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string FullName { get; set; } = null!;
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public bool IsActive { get; set; }
    public ICollection<string> Roles { get; set; } = new List<string>();
    public ICollection<string> Permissions { get; set; } = new List<string>();
}
