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
        private string GetCurrentConnectionString()
        {
            ConfigurationManager.RefreshSection("connectionStrings");

            // Retornamos la cadena de conexión actualizada.
            return ConfigurationManager.ConnectionStrings["MiConexion"]?.ConnectionString;
        }

        // Ejecutar INSERT, UPDATE o DELETE con parámetros
        public bool Comando (string sql, params SqlParameter[] parametros)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(GetCurrentConnectionString()))
                {
                    conexion.Open();
                    SqlCommand cmd = new SqlCommand(sql, conexion);
                    cmd.Parameters.AddRange(parametros);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Ejecutar SELECT que devuelve un DataSet con parámetros
        public DataSet ConsultarDS(string sql, params SqlParameter[] parametros)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conexion = new SqlConnection(GetCurrentConnectionString()))
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

        public bool TestConnection()
        {
            string connectionString = GetCurrentConnectionString(); // Asegúrate de usar el nombre correcto de tu cadena de conexión

            if (string.IsNullOrEmpty(connectionString))
            {
                return false; // No hay cadena de conexión configurada
            }

            try
            {
                using (SqlConnection cn = new SqlConnection(connectionString))
                {
                    cn.Open(); // Intenta abrir la conexión
                    cn.Close(); // Si se abrió, ciérrala inmediatamente
                }
                return true; // Conexión exitosa
            }
            catch (SqlException ex)
            {
                // Puedes loguear el error si lo deseas, pero no es necesario mostrarlo al usuario aquí.
                Console.WriteLine($"Error de SQL al probar la conexión: {ex.Message}");
                return false; // Falló la conexión SQL
            }
            catch (Exception ex)
            {
                // Captura cualquier otra excepción general
                Console.WriteLine($"Error general al probar la conexión: {ex.Message}");
                return false;
            }
        }

    }
}
