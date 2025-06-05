using Microsoft.Owin;
using System.Xml.Linq;
using Owin;

[assembly: OwinStartup(typeof(WindowsFormsApp1.Inicio))]

namespace WindowsFormsApp1
{
    public class Inicio
    {
        public void Configuracion(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}