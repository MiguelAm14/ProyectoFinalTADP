using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Net;
using System.Threading.Tasks;
using WindowsFormsApp1;

public class NotificadorCambios : IDisposable
{
    private HubConnection _conexion;
    private IHubProxy _proxyHub;
    private IDisposable _servidor;

    public event Action AlDetectarCambio;

    public NotificadorCambios()
    {
        IniciarServidor();
    }

    private void IniciarServidor()
    {
        try
        {
            _servidor = WebApp.Start<Inicio>("http://*:8080");
        }
        catch { /* El puerto puede estar en uso */ }
    }

    public async Task ConectarANodoPrincipal(string ipPrincipal)
    {
        try
        {
            _conexion = new HubConnection($"http://{ipPrincipal}:8080");
            _proxyHub = _conexion.CreateHubProxy("HubCambiosDatos");

            _proxyHub.On("RefrescarDatos", () =>
            {
                AlDetectarCambio?.Invoke();
            });

            await _conexion.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error de conexión: {ex.Message}");
        }
    }

    public async Task NotificarCambio()
    {
        try
        {
            if (_proxyHub != null)
                await _proxyHub.Invoke("NotificarCambio");
        }
        catch { /* Manejo opcional de errores */ }
    }

    public void Dispose()
    {
        _servidor?.Dispose();
        _conexion?.Dispose();
    }

    public static string ObtenerIpLocal()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }
}
