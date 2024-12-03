using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class AccountConversationRepository : GenericRepository<AccountConversation>, IAccountConversationRepository
{
    public AccountConversationRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }

    public async Task<AccountConversation?> FindByAccountIdAndConversationIdAsync(Guid accountId, Guid conversationId)
    {
        return await _dbSet.Include(accountConversation => accountConversation.Conversation).FirstOrDefaultAsync(
            accountConversation => accountConversation.AccountId == accountId &&
                                   !accountConversation.Account.IsDeleted &&
                                   accountConversation.ConversationId == conversationId &&
                                   !accountConversation.IsDeleted);
    }

    public async Task<int> CountAllActiveMembersByConversationIdAsync(Guid conversationId)
    {
        return await _dbSet.Where(accountConversation => accountConversation.ConversationId == conversationId &&
                                                         !accountConversation.IsDeleted &&
                                                         !accountConversation.Account.IsDeleted)
            .Select(accountConversation => accountConversation.Account)
            .Distinct().CountAsync();
    }

    public async Task<List<Account>> GetAllActiveMembersByConversationIdAsync(Guid conversationId)
    {
        return await _dbSet.Where(accountConversation => accountConversation.ConversationId == conversationId &&
                                                         !accountConversation.IsDeleted &&
                                                         !accountConversation.Account.IsDeleted)
            .Select(accountConversation => accountConversation.Account)
            .Distinct().ToListAsync();
    }
}