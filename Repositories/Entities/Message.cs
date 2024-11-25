namespace Repositories.Entities;

public class Message : BaseEntity
{
    public string Body { get; set; } = null!;
    public string AttachmentUrl { get; set; } = null!;
    public bool IsPinned { get; set; } = false;

    // Foreign key
    public Guid AccountId { get; set; }
    public Guid? ParentMessageId { get; set; }

    // Relationship
    public Account Account { get; set; } = null!;
    public Message? ParentMessage { get; set; }
}