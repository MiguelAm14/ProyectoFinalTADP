using Microsoft.Owin;
using System.Xml.Linq;
using Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using System;

[assembly: OwinStartup(typeof(WindowsFormsApp1.Inicio))]

namespace WindowsFormsApp1
{
    public class Inicio
    {
        public void Configuration(IAppBuilder app)
        {
            // Configuración del servidor SignalR
            app.UseCors(CorsOptions.AllowAll);

            var hubConfiguration = new HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJSONP = true,
                EnableJavaScriptProxies = true
            };

            // Configurar timeouts
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(30);
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(30);
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(10);

            app.MapSignalR("/signalr", hubConfiguration);
        }
    }
}