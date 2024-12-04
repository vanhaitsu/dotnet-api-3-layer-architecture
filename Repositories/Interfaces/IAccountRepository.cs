using Repositories.Entities;

namespace Repositories.Interfaces;

public interface IAccountRepository : IGenericRepository<Account>
{
    Task<Account?> FindByEmailAsync(string email, string? include = "");
    Task<Account?> FindByUsernameAsync(string username, string? include = "");
    Task<List<Guid>> GetValidAccountIdsAsync(List<Guid> accountIds);
}