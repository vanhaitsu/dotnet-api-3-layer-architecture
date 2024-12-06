using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class AccountRepository : GenericRepository<Account>, IAccountRepository
{
    public AccountRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }

    public async Task<Account?> FindByEmailAsync(string email, string? include = "")
    {
        IQueryable<Account> query = _dbSet;
        if (!string.IsNullOrWhiteSpace(include))
            foreach (var includeProperty in include.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(includeProperty.Trim());

        return await query.FirstOrDefaultAsync(account => account.Email == email);
    }

    public async Task<Account?> FindByUsernameAsync(string username, string? include = "")
    {
        IQueryable<Account> query = _dbSet;
        if (!string.IsNullOrWhiteSpace(include))
            foreach (var includeProperty in include.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(includeProperty.Trim());

        return await query.FirstOrDefaultAsync(account => account.Username == username);
    }

    public async Task<List<Guid>> GetValidAccountIdsAsync(List<Guid> accountIds)
    {
        return await _dbSet.Where(account => accountIds.Contains(account.Id) && !account.IsDeleted)
            .Select(account => account.Id).Distinct().ToListAsync();
    }
}