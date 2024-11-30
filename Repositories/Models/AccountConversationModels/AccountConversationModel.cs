using Repositories.Entities;
using Repositories.Models.AccountModels;

namespace Repositories.Models.AccountConversationModels;

public class AccountConversationModel : BaseEntity
{
    public bool IsArchived { get; set; }
    public bool IsOwner { get; set; }
    
    // Relationship
    public SimpleAccountModel Account { get; set; } = null!;
}