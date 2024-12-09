using Repositories.Entities;
using Repositories.Enums;
using Repositories.Models.AccountModels;

namespace Repositories.Models.MessageModels;

public class MessageModel : BaseEntity
{
    public string? Content { get; set; }
    public string? AttachmentUrl { get; set; }
    public MessageType MessageType { get; set; }
    public bool IsPinned { get; set; }
    public bool IsModified { get; set; }

    // Foreign key
    public Guid? ParentMessageId { get; set; }

    // Relationship
    public List<AccountLiteModel>? IsReadBy { get; set; }
}