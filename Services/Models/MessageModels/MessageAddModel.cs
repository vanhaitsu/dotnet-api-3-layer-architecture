using System.ComponentModel.DataAnnotations;

namespace Services.Models.MessageModels;

public class MessageAddModel
{
    [Required] public Guid ConversationId { get; set; }
    [Required] public string Body { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public Guid? ParentMessageId { get; set; }
}