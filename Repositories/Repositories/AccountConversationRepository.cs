using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class AccountConversationRepository : GenericRepository<AccountConversation>, IAccountConversationRepository
{
    public AccountConversationRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }

    public async Task<AccountConversation?> FindByAccountIdAndConversationIdAsync(Guid accountId, Guid conversationId,
        Func<IQueryable<AccountConversation>, IQueryable<AccountConversation>>? include = null)
    {
        IQueryable<AccountConversation> query = _dbSet;
        if (include != null) query = include(query);

        return await query.FirstOrDefaultAsync(accountConversation =>
            accountConversation.ConversationId == conversationId && accountConversation.AccountId == accountId &&
            !accountConversation.IsDeleted);
    }
}