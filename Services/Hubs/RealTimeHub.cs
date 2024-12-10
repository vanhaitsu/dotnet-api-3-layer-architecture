using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Services.Hubs;

public class RealTimeHub : Hub
{
    private readonly ConnectionMapping<Guid> _connections = new();

    public override async Task OnConnectedAsync()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId.HasValue)
        {
            var connectionId = Context.ConnectionId;
            _connections.Add(currentUserId.Value, connectionId);
            await base.OnConnectedAsync();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId.HasValue)
        {
            var connectionId = Context.ConnectionId;
            _connections.Remove(currentUserId.Value, connectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }

    # region Helper

    private Guid? GetCurrentUserId()
    {
        var identity = Context.User?.Identity as ClaimsIdentity;
        var accountIdClaim = identity?.FindFirst("accountId");
        if (accountIdClaim != null && Guid.TryParse(accountIdClaim.Value, out var currentUserId)) return currentUserId;

        return null;
    }

    #endregion
}