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
        return await _dbSet.Include(accountConversation => accountConversation.Conversation)
            .Include(accountConversation => accountConversation.MessageRecipients)
            .ThenInclude(messageRecipient => messageRecipient.Message)
            .FirstOrDefaultAsync(accountConversation => accountConversation.AccountId == accountId &&
                                                        !accountConversation.Account.IsDeleted &&
                                                        accountConversation.ConversationId == conversationId &&
                                                        !accountConversation.IsDeleted);
    }
}