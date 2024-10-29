using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class AccountRoleRepository : GenericRepository<AccountRole>, IAccountRoleRepository
{
    public AccountRoleRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }
}