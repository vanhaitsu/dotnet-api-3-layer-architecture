using Repositories.Entities;

namespace Repositories.Interfaces;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> FindByNameAsync(string name);
    Task<List<Role>> GetAllByAccountIdAsync(Guid accountId);
}