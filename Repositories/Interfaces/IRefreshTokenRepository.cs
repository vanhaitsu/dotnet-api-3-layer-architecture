using Repositories.Entities;

namespace Repositories.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> FindByDeviceIdAsync(Guid deviceId);
}