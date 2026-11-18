using Microsoft.AspNetCore.SignalR;

namespace CongoBiz.API.Hubs;

public class OrderHub : Hub
{
    public async Task JoinRestaurantGroup(string restaurantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, restaurantId);
    }

    public async Task LeaveRestaurantGroup(string restaurantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, restaurantId);
    }
}