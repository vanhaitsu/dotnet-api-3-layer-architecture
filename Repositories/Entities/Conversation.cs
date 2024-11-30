namespace Repositories.Entities;

public class Conversation : BaseEntity
{
    public string? Name { get; set; }
    public string? Image { get; set; }
    public bool IsRestricted { get; set; } = false;

    // Relationship
    public virtual ICollection<AccountConversation> AccountConversations { get; set; } =
        new List<AccountConversation>();
}