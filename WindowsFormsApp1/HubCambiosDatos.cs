using System.Threading.Tasks;
using System;
using Microsoft.AspNet.SignalR;

public class HubCambiosDatos : Hub
{
    private static IHubContext _context = GlobalHost.ConnectionManager.GetHubContext<HubCambiosDatos>();

    public void NotificarCambioGlobal()
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Notificación enviada desde {Context.ConnectionId}");
        _context.Clients.All.ActualizarClientes();
    }

    public void ProbarEco(string mensaje)
    {
        Console.WriteLine($"Eco recibido: {mensaje}");
        Clients.Caller.RecibirEco($"Servidor respondió: {mensaje} - {DateTime.Now}");
    }

    public override Task OnConnected()
    {
        Console.WriteLine($"Cliente conectado: {Context.ConnectionId}");
        return base.OnConnected();
    }

    public override Task OnDisconnected(bool stopCalled)
    {
        Console.WriteLine($"Cliente desconectado: {Context.ConnectionId}");
        return base.OnDisconnected(stopCalled);
    }
}