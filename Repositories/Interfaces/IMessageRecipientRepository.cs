using Repositories.Entities;

namespace Repositories.Interfaces;

public interface IMessageRecipientRepository : IGenericRepository<MessageRecipient>
{
    Task SoftRemoveAllByAccountIdAndAccountConversationIdAsync(Guid accountId, Guid accountConversationId);
}