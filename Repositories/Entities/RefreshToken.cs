namespace Repositories.Entities;

public class RefreshToken : BaseEntity
{
    public Guid DeviceId { get; set; }
    public string Token { get; set; } = null!;

    // Foreign key
    public Guid AccountId { get; set; }

    // Relationship
    public Account Account { get; set; } = null!;
}