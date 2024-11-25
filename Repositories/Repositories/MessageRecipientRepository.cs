using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class MessageRecipientRepository : GenericRepository<MessageRecipient>, IMessageRecipientRepository
{
    public MessageRecipientRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }
}