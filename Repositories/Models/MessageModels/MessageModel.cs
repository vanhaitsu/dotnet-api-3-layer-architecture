using Repositories.Entities;
using Repositories.Enums;

namespace Repositories.Models.MessageModels;

public class MessageModel : BaseEntity
{
    public string Content { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public MessageType MessageType { get; set; }
    public bool IsPinned { get; set; }

    // Foreign key
    public Guid? ParentMessageId { get; set; }
}