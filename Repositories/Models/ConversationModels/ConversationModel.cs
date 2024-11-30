using Repositories.Entities;

namespace Repositories.Models.ConversationModels;

public class ConversationModel : BaseEntity
{
    public string? Name { get; set; }
    public string? Image { get; set; }
    public bool IsRestricted { get; set; }
    public bool IsGroup { get; set; }
    public int NumberOfMembers { get; set; }
    public bool IsAllowed { get; set; } // Allow current user to send message or not
}