namespace Repositories.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Relationship
    public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();
}