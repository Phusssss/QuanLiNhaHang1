using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class OrderHub : Hub
{
    public async Task SendOrderNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveOrderNotification", message);
    }
}
