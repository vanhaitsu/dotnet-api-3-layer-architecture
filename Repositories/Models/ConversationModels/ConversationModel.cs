using Repositories.Entities;
using Repositories.Models.AccountModels;

namespace Repositories.Models.ConversationModels;

public class ConversationModel : BaseEntity
{
    public string? Name { get; set; }
    public string? Image { get; set; }
    public bool IsRestricted { get; set; }
    public bool IsActive { get; set; }
    public bool IsGroup { get; set; }
    public int NumberOfMembers { get; set; }

    // Relationship
    public List<MemberModel> Members { get; set; } = null!;
}