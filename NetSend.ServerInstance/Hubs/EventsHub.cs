using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NetSend.ServerInstance.Hubs;

public class EventsHub : Hub
{
    [HubMethodName("send_message")]
    [Authorize]
    public async Task SendMessageAsync(Guid channelId, string message)
    {
    }
}