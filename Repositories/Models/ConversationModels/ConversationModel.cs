using Repositories.Entities;
using Repositories.Models.AccountModels;
using Repositories.Models.MessageModels;

namespace Repositories.Models.ConversationModels;

public class ConversationModel : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Image { get; set; }
    public bool IsRestricted { get; set; }
    public bool IsActive { get; set; }
    public bool IsGroup { get; set; }
    public int NumberOfMembers { get; set; }
    public int? NumberOfUnreadMessages { get; set; }

    // Relationship
    public List<MemberModel> Members { get; set; } = null!;
    public LatestMessageModel? LatestMessage { get; set; }
}