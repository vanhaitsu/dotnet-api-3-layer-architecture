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
}