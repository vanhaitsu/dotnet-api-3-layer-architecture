using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
{
    public ConversationRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }

    public async Task<Conversation?> FindByAccountIdsAsync(List<Guid> accountIds)
    {
        return await _dbSet.FirstOrDefaultAsync(conversation =>
            conversation.AccountConversations.Where(accountConversation => !accountConversation.Account.IsDeleted)
                .Select(accountConversation => accountConversation.Id).Count() == accountIds.Count &&
            accountIds.Except(conversation.AccountConversations.Select(accountConversation => accountConversation.Id))
                .Any());
    }
}