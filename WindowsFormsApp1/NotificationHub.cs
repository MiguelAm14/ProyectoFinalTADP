using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class NotificationHub : Hub
    {
        public async Task NotificarCambio()
        {
            await Clients.All.ActualizarDatos();
        }

        // Método adicional para debugging
        public override async Task OnConnected()
        {
            Console.WriteLine($"Cliente conectado: {Context.ConnectionId}");
            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            Console.WriteLine($"Cliente desconectado: {Context.ConnectionId}");
            await base.OnDisconnected(stopCalled);
        }

    }
}
