using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }

    public async Task<Role?> FindByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(role => role.Name == name);
    }

    public async Task<List<Role>> GetAllByAccountIdAsync(Guid accountId)
    {
        return await _dbSet.Where(x => x.AccountRoles.Any(role => role.AccountId == accountId)).ToListAsync();
    }
}