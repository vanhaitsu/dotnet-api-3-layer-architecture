namespace Repositories.Entities;

public class AccountConversation : BaseEntity
{
    public bool IsArchived { get; set; } = false;
    public bool IsOwner { get; set; } = false;

    // Foreign key
    public Guid AccountId { get; set; }
    public Guid ConversationId { get; set; }

    // Relationship
    public Account Account { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;
}