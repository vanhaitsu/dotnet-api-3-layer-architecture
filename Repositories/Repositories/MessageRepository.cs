using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }
}