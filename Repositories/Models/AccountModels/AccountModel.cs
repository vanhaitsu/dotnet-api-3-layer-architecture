using Repositories.Entities;
using Repositories.Enums;
using Role = Repositories.Enums.Role;

namespace Repositories.Models.AccountModels;

public class AccountModel : BaseEntity
{
    // Required information
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;

    // Personal information
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Image { get; set; }

    // Status
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }

    // Relationship
    public List<Role> Roles { get; set; } = null!;
    public List<string> RoleNames { get; set; } = null!;
}