using Repositories.Entities;

namespace Repositories.Interfaces;

public interface IAccountConversationRepository : IGenericRepository<AccountConversation>
{
    Task<AccountConversation?> FindByAccountIdAndConversationIdAsync(Guid accountId, Guid conversationId);
}