using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using WindowsFormsApp1;

public class NotificadorCambios : IDisposable
{
    private HubConnection _conexion;
    private IHubProxy _proxyHub;
    private string _urlServidor;

    public event Action OnCambioRecibido;

    public NotificadorCambios(string urlServidor)
    {
        _urlServidor = urlServidor ?? throw new ArgumentNullException(nameof(urlServidor));
    }

    public async Task IniciarConexion()
    {
        try
        {
            // Verificar conectividad primero
            if (!await VerificarConectividadServidor())
            {
                throw new Exception("No se puede alcanzar el servidor SignalR");
            }

            _conexion = new HubConnection(_urlServidor)
            {
                TraceLevel = TraceLevels.All,
                TraceWriter = Console.Out
            };

            _proxyHub = _conexion.CreateHubProxy("HubCambiosDatos");

            _proxyHub.On("ActualizarClientes", () =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Notificación recibida");
                OnCambioRecibido?.Invoke();
            });

            _proxyHub.On<string>("RecibirConfirmacion", (mensaje) =>
            {
                Console.WriteLine($"[Confirmación] {mensaje}");
            });

            _conexion.Closed += async () =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Conexión cerrada. Reconectando...");
                await Task.Delay(new Random().Next(2000, 5000));
                await IniciarConexion();
            };

            await _conexion.Start();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Conexión establecida con {_urlServidor}");

            // Probar conexión
            await _proxyHub.Invoke("ProbarEco", "¡Hola desde el cliente!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR en IniciarConexion: {ex}");
            throw;
        }
    }

    private async Task<bool> VerificarConectividadServidor()
    {
        try
        {
            var uri = new Uri(_urlServidor);
            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(uri.Host, uri.Port);
                return tcpClient.Connected;
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task NotificarCambio()
    {
        try
        {
            if (_conexion?.State == ConnectionState.Connected)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Enviando notificación...");
                await _proxyHub.Invoke("NotificarCambioGlobal");
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] No se puede notificar - Estado: {_conexion?.State}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR en NotificarCambio: {ex}");
            throw;
        }
    }

    public void Dispose()
    {
        _conexion?.Dispose();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Notificador liberado");
    }
}
