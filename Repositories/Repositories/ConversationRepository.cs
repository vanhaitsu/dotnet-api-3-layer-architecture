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
            conversation.AccountConversations.Count(accountConversation => !accountConversation.IsDeleted) ==
            accountIds.Count &&
            !accountIds.Except(
                conversation.AccountConversations.Select(accountConversation => accountConversation.AccountId)).Any());
    }

    public async Task<Conversation?> FindByAccountIdAndConversationIdAsync(Guid accountId, Guid conversationId,
        Func<IQueryable<Conversation>, IQueryable<Conversation>>? include = null)
    {
        IQueryable<Conversation> query = _dbSet;
        if (include != null) query = include(query);

        return await query.FirstOrDefaultAsync(conversation =>
            conversation.Id == conversationId && conversation.AccountConversations.Any(accountConversation =>
                accountConversation.AccountId == accountId && !accountConversation.IsDeleted));
    }
}