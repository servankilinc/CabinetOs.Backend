using System.ComponentModel;

namespace CabinetOs.Core.Enums;

public enum RoleType
{
    [Description("User")]
    User = 1,
    [Description("Manager")]
    Manager = 2,
    [Description("Admin")]
    Admin = 3,
    [Description("Owner")]
    Owner = 4,
}