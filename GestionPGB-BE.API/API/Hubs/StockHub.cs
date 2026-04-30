using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GestionPGB_BE.API.API.Hubs;

[Authorize]
public class StockHub(ILogger<StockHub> logger) : Hub
{
    public async Task JoinSession(string mode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, mode);
    }

    public async Task SetMode(string mode)
    {
        await Clients.Others.SendAsync("ModeChanged", mode);
    }

    public async Task BroadcastBarcode(string barcode)
    {
        await Clients.Others.SendAsync("ReceiveBarcode", barcode);
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.User?.Identity?.Name ?? "anónimo";
        logger.LogInformation("SignalR conectado: {ConnectionId} | Usuario: {User}", Context.ConnectionId, user);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Context.User?.Identity?.Name ?? "anónimo";
        if (exception is null)
            logger.LogInformation("SignalR desconectado: {ConnectionId} | Usuario: {User}", Context.ConnectionId, user);
        else
            logger.LogWarning(exception, "SignalR desconectado con error: {ConnectionId} | Usuario: {User}", Context.ConnectionId, user);

        await base.OnDisconnectedAsync(exception);
    }
}
