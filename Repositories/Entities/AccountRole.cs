namespace Repositories.Entities;

public class AccountRole : BaseEntity
{
    // Foreign key
    public Guid AccountId { get; set; }
    public Guid RoleId { get; set; }

    // Relationship
    public Account Account { get; set; } = null!;
    public Role Role { get; set; } = null!;
}