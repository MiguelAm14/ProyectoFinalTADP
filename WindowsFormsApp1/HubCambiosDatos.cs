using Microsoft.AspNet.SignalR;

public class HubCambiosDatos : Hub
{
    public void NotificarCambio()
    {
        Clients.All.RefrescarDatos();
    }
}