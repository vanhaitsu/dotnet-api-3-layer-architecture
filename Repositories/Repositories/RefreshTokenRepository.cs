using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }

    public async Task<RefreshToken?> FindByDeviceIdAsync(Guid deviceId)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.DeviceId == deviceId);
    }
}