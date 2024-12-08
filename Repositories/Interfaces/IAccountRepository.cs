using Repositories.Entities;

namespace Repositories.Interfaces;

public interface IAccountRepository : IGenericRepository<Account>
{
    Task<Account?> FindByEmailAsync(string email, Func<IQueryable<Account>, IQueryable<Account>>? include = null);
    Task<Account?> FindByUsernameAsync(string username, Func<IQueryable<Account>, IQueryable<Account>>? include = null);
    Task<List<Guid>> GetValidAccountIdsAsync(List<Guid> accountIds);
}