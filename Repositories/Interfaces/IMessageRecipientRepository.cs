using Repositories.Entities;

namespace Repositories.Interfaces;

public interface IMessageRecipientRepository : IGenericRepository<MessageRecipient>
{
    Task SoftRemoveAllByAccountIdAndAccountConversationIdAsync(Guid accountId, Guid accountConversationId);
    Task<MessageRecipient?> FindByAccountIdAndMessageIdAsync(Guid accountId, Guid messageId);
    Task UpdateIsReadAllByAccountIdAndConversationIdAsync(Guid accountId, Guid conversationId);
}