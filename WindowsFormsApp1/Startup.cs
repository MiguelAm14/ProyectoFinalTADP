using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

namespace WindowsFormsApp1
{
    internal class Startup
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
    }
}
