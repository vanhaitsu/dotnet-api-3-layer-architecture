using Repositories.Entities;

namespace Repositories.Interfaces;

public interface IConversationRepository : IGenericRepository<Conversation>
{
    Task<Conversation?> FindByAccountIdsAsync(List<Guid> accountIds);
}