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
