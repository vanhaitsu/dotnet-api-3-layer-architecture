using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class AccountConversationRepository : GenericRepository<AccountConversation>, IAccountConversationRepository
{
    public AccountConversationRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }
}