using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class formConfig: Form
    {
        public formConfig()
        {
            InitializeComponent();
            CargarConfiguracion();
        }

        private void formConfig_Load(object sender, EventArgs e)
        {
            // Diseño del botón insertar
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 2;
            btnGuardar.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#FFFFFF");
            btnGuardar.BackColor = ColorTranslator.FromHtml("#0078D7");
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Font = new Font("Roboto", 10, FontStyle.Bold);
            btnGuardar.Cursor = Cursors.Hand;

            txtContrasena.PasswordChar = '*';
        }

        private void CargarConfiguracion()
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["MiConexion"]?.ConnectionString;
                string signalRUrl = ConfigurationManager.AppSettings["SignalRServerUrl"];

                if (connStr != null)
                {
                    var builder = new SqlConnectionStringBuilder(connStr);
                    txtURL.Text = builder.DataSource;
                    txtDB.Text = builder.InitialCatalog;
                    txtUsuario.Text = builder.UserID;
                    txtContrasena.Text = builder.Password;
                }

                txtURLSignal.Text = signalRUrl;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar configuración: " + ex.Message);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Crear cadena de conexión nueva
                string nuevaConexion = $"Server={txtURL.Text};Database={txtDB.Text};User Id={txtUsuario.Text};Password={txtContrasena.Text};";

                // Abrir configuración
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // Guardar cadena de conexión
                if (config.ConnectionStrings.ConnectionStrings["MiConexion"] != null)
                {
                    config.ConnectionStrings.ConnectionStrings["MiConexion"].ConnectionString = nuevaConexion;
                }
                else
                {
                    config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("MiConexion", nuevaConexion, "System.Data.SqlClient"));
                }

                // Guardar URL de SignalR
                if (config.AppSettings.Settings["SignalRServerUrl"] != null)
                {
                    config.AppSettings.Settings["SignalRServerUrl"].Value = txtURLSignal.Text;
                }
                else
                {
                    config.AppSettings.Settings.Add("SignalRServerUrl", txtURLSignal.Text);
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("connectionStrings");
                ConfigurationManager.RefreshSection("appSettings");

                MessageBox.Show("Configuración guardada correctamente.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar configuración: " + ex.Message);
            }
        }
    }
}
