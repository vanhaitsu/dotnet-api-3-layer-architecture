using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
{
    public ConversationRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }
}