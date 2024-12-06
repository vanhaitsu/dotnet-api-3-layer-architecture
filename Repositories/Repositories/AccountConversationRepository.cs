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

    // public async Task<ConversationModel?> FindAndMapByAccountIdAndConversationIdAsync(Guid accountId,
    //     Guid conversationId)
    // {
    //     var result = await _dbSet
    //         .Where(accountConversation => accountConversation.AccountId == accountId &&
    //                                       !accountConversation.Account.IsDeleted &&
    //                                       accountConversation.ConversationId == conversationId &&
    //                                       !accountConversation.IsDeleted)
    //         
    //         .Select(accountConversation => new ConversationModel
    //         {
    //             Id = accountConversation.Id,
    //             IsRestricted = accountConversation.Conversation.IsRestricted,
    //             NumberOfUnreadMessages = accountConversation.MessageRecipients.Count(messageRecipient =>
    //                 messageRecipient.AccountId == accountId && !messageRecipient.IsRead),
    //             IsArchived = accountConversation.IsArchived,
    //             IsOwner = accountConversation.IsOwner,
    //             LatestMessage = accountConversation.MessageRecipients
    //                 .Select(messageRecipient => messageRecipient.Message)
    //                 .OrderByDescending(message => message.CreationDate)
    //                 .Select(message => new MessageModel
    //                 {
    //                     Id = message.Id,
    //                     Content = message.Content,
    //                     AttachmentUrl = message.AttachmentUrl,
    //                     MessageType = message.MessageType,
    //                     IsPinned = message.IsPinned,
    //                     IsRead = message.IsRead,
    //                     CreatedBy = message.CreatedBy,
    //                     CreationDate = message.CreationDate,
    //                 })
    //                 .FirstOrDefault(),
    //             CreationDate = accountConversation.CreationDate,
    //         })
    //         .FirstOrDefaultAsync();
    // }
}