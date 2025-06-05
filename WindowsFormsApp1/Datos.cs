using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.AspNet.SignalR.Client;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    class Datos
    {
        private string cadena = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
        private static HubConnection connection;
        private static IHubProxy hubProxy;
        private static bool isInitialized = false;

        static Datos()
        {
            if (!isInitialized)
            {
                InicializarSignalR();
                isInitialized = true;
            }
        }

        private static async void InicializarSignalR()
        {
            try
            {
                connection = new HubConnection("http://IP_O_DOMINIO_DEL_SERVIDOR:PUERTO/");
                hubProxy = connection.CreateHubProxy("NotificacionHub");

                // Configurar reconexión automática también aquí
                connection.Closed += async () =>
                {
                    await Task.Delay(3000);
                    try
                    {
                        await connection.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reconectando en Datos: {ex.Message}");
                    }
                };

                await connection.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inicializando SignalR en Datos: {ex.Message}");
            }
        }

        private static async void NotificarCambio()
        {
            if (connection?.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
            {
                try
                {
                    await hubProxy.Invoke("NotificarCambio");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error notificando cambio: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("SignalR no conectado, reintentando...");
                try
                {
                    await connection.Start();
                    await hubProxy.Invoke("NotificarCambio");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reintentando notificación: {ex.Message}");
                }
            }
        }

        // Ejecutar INSERT, UPDATE o DELETE con parámetros
        public bool Comando (string sql, params SqlParameter[] parametros)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();
                    SqlCommand cmd = new SqlCommand(sql, conexion);
                    cmd.Parameters.AddRange(parametros);
                    cmd.ExecuteNonQuery();
                }
                NotificarCambio();
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public static void CerrarConexionSignalR()
        {
            try
            {
                connection?.Stop();
                connection?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cerrando conexión estática: {ex.Message}");
            }
        }


        // Ejecutar SELECT que devuelve un DataSet con parámetros
        public DataSet ConsultarDS(string sql, params SqlParameter[] parametros)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conexion))
                    {
                        cmd.Parameters.AddRange(parametros);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al consultar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return ds;
        }

    }
}
