using System.Security.Claims;
using Repositories.Interfaces;

namespace API.Utils;

public class ClaimService : IClaimService
{
    public ClaimService(IHttpContextAccessor httpContextAccessor)
    {
        var identity = httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity;
        var userIdClaim = identity?.FindFirst("accountId");
        if (userIdClaim != null) GetCurrentUserId = new Guid(userIdClaim.Value);
    }

    public Guid? GetCurrentUserId { get; }
}