using Repositories.Enums;

namespace Repositories.Entities;

public class Message : BaseEntity
{
    public string Content { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public MessageType MessageType { get; set; } = MessageType.Text;
    public bool IsPinned { get; set; } = false;

    // Foreign key
    public Guid? ParentMessageId { get; set; }

    // Relationship
    public Account CreatedBy { get; set; } = null!;
    public Message? ParentMessage { get; set; }
    public virtual ICollection<MessageRecipient> MessageRecipients { get; set; } = new List<MessageRecipient>();
}