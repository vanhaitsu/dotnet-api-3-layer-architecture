using Repositories.Entities;

namespace Repositories.Models.AccountModels;

public class SimpleAccountModel : BaseEntity
{
    // Required information
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;

    // Personal information
    public string? Username { get; set; }
    public string? Image { get; set; }
}