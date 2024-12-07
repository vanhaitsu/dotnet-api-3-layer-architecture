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
            conversation.AccountConversations.Where(accountConversation => !accountConversation.IsDeleted)
                .Select(accountConversation => accountConversation.Id).Count() == accountIds.Count &&
            accountIds.Except(conversation.AccountConversations.Select(accountConversation => accountConversation.Id))
                .Any());
    }

    public async Task<Conversation?> FindByAccountIdAndConversationIdAsync(Guid accountId, Guid conversationId)
    {
        return await _dbSet.Include(conversation => conversation.AccountConversations)
            .ThenInclude(accountConversation => accountConversation.Account)
            .Include(conversation => conversation.AccountConversations).ThenInclude(accountConversation =>
                accountConversation.MessageRecipients.Where(messageRecipient => !messageRecipient.IsDeleted)
                    .OrderByDescending(messageRecipient => messageRecipient.Message.CreationDate).Take(6))
            .ThenInclude(messageRecipient => messageRecipient.Message).FirstOrDefaultAsync(conversation =>
                conversation.Id == conversationId && conversation.AccountConversations.Any(accountConversation =>
                    accountConversation.AccountId == accountId && !accountConversation.IsDeleted));
    }
}