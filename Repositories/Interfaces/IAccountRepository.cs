using Repositories.Entities;

namespace Repositories.Interfaces;

public interface IAccountRepository : IGenericRepository<Account>
{
    Task<List<Guid>> GetValidAccountIdsAsync(List<Guid> accountIds);

    #region Authentication

    Task<Account?> FindByEmailAsync(string email);
    Task<Account?> FindByUsernameAsync(string username);

    #endregion
}