using Repositories.Entities;
using Repositories.Enums;

namespace Repositories.Models.MessageModels;

public class MessageModel : BaseEntity
{
    public string Content { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public MessageType MessageType { get; set; }
    public bool IsPinned { get; set; }
    public bool IsRead { get; set; }

    // Relationship
    public Account CreatedBy { get; set; } = null!;
    public Message? ParentMessage { get; set; }
}