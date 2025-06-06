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
        private formMain main;

        public formConfig(formMain main)
        {
            InitializeComponent();
            CargarConfiguracion();
            this.main = main;
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

        private void CargarEstadoAutenticacion()
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["MiConexion"]?.ConnectionString;

                if (connStr != null)
                {
                    // Crear un SqlConnectionStringBuilder para analizar la cadena de conexión
                    var builder = new SqlConnectionStringBuilder(connStr);

                    // Si usa Integrated Security, marcar el checkbox
                    if (builder.IntegratedSecurity) // Esto es True si Integrated Security=True;
                    {
                        chkWindowsAuth.Checked = true;
                    }
                    else
                    {
                        // Si no usa Integrated Security, significa que usa usuario/contraseña SQL
                        chkWindowsAuth.Checked = false;
                    }

                    // Habilitar/Deshabilitar los campos según el estado inicial
                    SetAuthenticationFieldsState(chkWindowsAuth.Checked);
                }
                else
                {
                    // Si no hay una cadena de conexión guardada, asumir SQL Server Auth por defecto
                    // o puedes elegir que Windows Auth sea el default
                    chkWindowsAuth.Checked = false; // O true, según tu preferencia
                    SetAuthenticationFieldsState(chkWindowsAuth.Checked);
                }
            }
            catch (Exception ex)
            {
                // Manejar errores al analizar la cadena de conexión
                MessageBox.Show("Error al cargar el estado de autenticación: " + ex.Message);
                // Si hay un error, puedes establecer un estado predeterminado
                chkWindowsAuth.Checked = false;
                SetAuthenticationFieldsState(chkWindowsAuth.Checked);
            }
        }

        private void chkWindowsAuth_CheckedChanged(object sender, EventArgs e)
        {
            SetAuthenticationFieldsState(chkWindowsAuth.Checked);
        }

        private void SetAuthenticationFieldsState(bool useWindowsAuth)
        {
            txtUsuario.Enabled = !useWindowsAuth;
            txtContrasena.Enabled = !useWindowsAuth;

            // Opcional: limpiar los campos si se cambia a autenticación de Windows
            if (useWindowsAuth)
            {
                txtUsuario.Text = "";
                txtContrasena.Text = "";
            }
        }

        private void CargarConfiguracion() // Se mantiene casi igual, solo se remueve la lógica de inicialización de chkWindowsAuth
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

                    // Si es autenticación de SQL Server, cargar usuario y contraseña
                    if (!builder.IntegratedSecurity)
                    {
                        txtUsuario.Text = builder.UserID;
                        txtContrasena.Text = builder.Password;
                    }
                    // Si es Windows Auth, los campos de usuario/contraseña se quedarán vacíos (o se limpiarán después)
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
                string nuevaConexion;
                string providerName = "System.Data.SqlClient"; // El proveedor siempre es el mismo

                // Lógica para construir la cadena de conexión según la opción seleccionada
                if (chkWindowsAuth.Checked)
                {
                    // Conexión con Autenticación de Windows
                    nuevaConexion = $"Server={txtURL.Text};Database={txtDB.Text};Integrated Security=True;";
                }
                else
                {
                    // Conexión con Autenticación de SQL Server (IP/Nombre, Usuario, Contraseña)
                    nuevaConexion = $"Server={txtURL.Text};Database={txtDB.Text};User Id={txtUsuario.Text};Password={txtContrasena.Text};";
                }

                // Abrir configuración
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // Guardar cadena de conexión
                if (config.ConnectionStrings.ConnectionStrings["MiConexion"] != null)
                {
                    config.ConnectionStrings.ConnectionStrings["MiConexion"].ConnectionString = nuevaConexion;
                    config.ConnectionStrings.ConnectionStrings["MiConexion"].ProviderName = providerName;
                }
                else
                {
                    config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("MiConexion", nuevaConexion, providerName));
                }

                // Guardar URL de SignalR (esta parte se mantiene igual)
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
                main.CargarDatos(); // Llamar al método para recargar los datos en el formulario principal
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar configuración: " + ex.Message);
            }
        }
    }
}
