using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Hosting;
using System.Net;
using System.Net.Sockets;

[assembly: OwinStartupAttribute(typeof(WindowsFormsApp1.Startup))]
namespace WindowsFormsApp1
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            // Configuración adicional para mejor rendimiento
            var hubConfiguration = new Microsoft.AspNet.SignalR.HubConfiguration()
            {
                EnableDetailedErrors = true // Solo para desarrollo, quitar en producción
            };

            app.MapSignalR(hubConfiguration);
        }

        // Método para obtener la IP local
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No se encontró una dirección IP local!");
        }

        // Método para iniciar el servidor con IP específica
        public static IDisposable StartServer(int port = 8080)
        {
            string localIP = GetLocalIPAddress();
            string url = $"http://{localIP}:{port}";

            try
            {
                var server = WebApp.Start<Startup>(url);
                Console.WriteLine($"Servidor SignalR iniciado en: {url}");
                Console.WriteLine($"Los clientes pueden conectarse usando: {url}");
                return server;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar el servidor: {ex.Message}");
                // Intenta con localhost como fallback
                //url = $"http://localhost:{port}";
                var server = WebApp.Start<Startup>(url);
                Console.WriteLine($"Servidor iniciado en modo local: {url}");
                return server;
            }
        }
    }
}