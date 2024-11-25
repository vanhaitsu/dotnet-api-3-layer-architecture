using Repositories.Enums;

namespace Repositories.Entities;

public class MessageRecipient : BaseEntity
{
    public MessageReaction? MessageReaction { get; set; }
    public bool IsRead { get; set; } = false;

    // Foreign key
    public Guid AccountId { get; set; }
    public Guid AccountConversationId { get; set; }
    public Guid MessageId { get; set; }

    // Relationship
    public Account Account { get; set; } = null!;
    public AccountConversation AccountConversation { get; set; } = null!;
    public Message Message { get; set; } = null!;
}