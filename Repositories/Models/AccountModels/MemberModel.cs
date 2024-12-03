using Repositories.Entities;

namespace Repositories.Models.AccountModels;

/// <summary>
///     This is a lite version of AccountModel
/// </summary>
public class MemberModel : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Image { get; set; }
}