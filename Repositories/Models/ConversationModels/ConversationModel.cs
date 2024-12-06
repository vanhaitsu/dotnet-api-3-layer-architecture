using Repositories.Entities;
using Repositories.Models.MessageModels;

namespace Repositories.Models.ConversationModels;

public class ConversationModel : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Image { get; set; }
    public bool IsRestricted { get; set; }
    public int NumberOfUnreadMessages { get; set; }
    public bool IsArchived { get; set; }
    public bool IsOwner { get; set; }

    // Relationship
    public MessageModel? LatestMessage { get; set; }
}