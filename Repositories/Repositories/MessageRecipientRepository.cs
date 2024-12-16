using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class MessageRecipientRepository : GenericRepository<MessageRecipient>, IMessageRecipientRepository
{
    public MessageRecipientRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }

    public async Task SoftRemoveAllByAccountIdAndAccountConversationIdAsync(Guid accountId, Guid accountConversationId)
    {
        var messageRecipients = await _dbSet.Where(messageRecipient =>
                messageRecipient.AccountId == accountId &&
                messageRecipient.AccountConversationId == accountConversationId && !messageRecipient.IsDeleted)
            .ToListAsync();
        SoftRemoveRange(messageRecipients);
    }

    public async Task<MessageRecipient?> FindByAccountIdAndMessageIdAsync(Guid accountId, Guid messageId)
    {
        return await _dbSet.FirstOrDefaultAsync(messageRecipient =>
            messageRecipient.AccountId == accountId && messageRecipient.MessageId == messageId);
    }

    public async Task UpdateIsReadAllByAccountIdAndConversationIdAsync(Guid accountId, Guid conversationId)
    {
        var messageRecipients = await _dbSet.Where(messageRecipient =>
                messageRecipient.AccountId == accountId &&
                messageRecipient.AccountConversation.ConversationId == conversationId && !messageRecipient.IsRead &&
                !messageRecipient.IsDeleted)
            .ToListAsync();
        foreach (var messageRecipient in messageRecipients) messageRecipient.IsRead = true;

        UpdateRange(messageRecipients);
    }
}