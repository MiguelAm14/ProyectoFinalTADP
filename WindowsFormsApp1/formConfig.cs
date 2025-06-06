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
    public partial class formConfig : Form
    {
        private formMain main;

        public formConfig(formMain main)
        {
            InitializeComponent();
            this.main = main;
            CargarConfiguracion();
            CargarEstadoAutenticacion(); // Llamar después de CargarConfiguracion
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
                    chkWindowsAuth.Checked = false;
                    SetAuthenticationFieldsState(chkWindowsAuth.Checked);
                }
            }
            catch (Exception ex)
            {
                // Manejar errores al analizar la cadena de conexión
                MessageBox.Show("Error al cargar el estado de autenticación: " + ex.Message);
                // Si hay un error, establecer un estado predeterminado
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

                    // Si es autenticación de SQL Server, cargar usuario y contraseña
                    if (!builder.IntegratedSecurity)
                    {
                        txtUsuario.Text = builder.UserID;
                        txtContrasena.Text = builder.Password;
                    }
                    // Si es Windows Auth, los campos de usuario/contraseña se quedarán vacíos
                }

                txtURLSignal.Text = signalRUrl;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar configuración: " + ex.Message);
            }
        }

        // HACER EL MÉTODO ASÍNCRONO
        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtURL.Text))
            {
                MessageBox.Show("Debe ingresar el servidor de base de datos.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtURL.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDB.Text))
            {
                MessageBox.Show("Debe ingresar el nombre de la base de datos.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDB.Focus();
                return;
            }

            if (!chkWindowsAuth.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtUsuario.Text))
                {
                    MessageBox.Show("Debe ingresar el usuario de SQL Server.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsuario.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtContrasena.Text))
                {
                    MessageBox.Show("Debe ingresar la contraseña de SQL Server.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtContrasena.Focus();
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(txtURLSignal.Text))
            {
                MessageBox.Show("Debe ingresar la URL del servidor SignalR.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtURLSignal.Focus();
                return;
            }

            // Validar formato de URL SignalR
            if (!txtURLSignal.Text.StartsWith("http://") && !txtURLSignal.Text.StartsWith("https://"))
            {
                MessageBox.Show("La URL de SignalR debe comenzar con http:// o https://", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtURLSignal.Focus();
                return;
            }

            if (!txtURLSignal.Text.EndsWith("/"))
            {
                txtURLSignal.Text += "/";
            }

            // Deshabilitar el botón mientras se procesa
            btnGuardar.Enabled = false;
            btnGuardar.Text = "Guardando...";

            try
            {
                string nuevaConexion;
                string providerName = "System.Data.SqlClient";

                // Lógica para construir la cadena de conexión según la opción seleccionada
                if (chkWindowsAuth.Checked)
                {
                    // Conexión con Autenticación de Windows
                    nuevaConexion = $"Server={txtURL.Text};Database={txtDB.Text};Integrated Security=True;Connection Timeout=30;";
                }
                else
                {
                    // Conexión con Autenticación de SQL Server
                    nuevaConexion = $"Server={txtURL.Text};Database={txtDB.Text};User Id={txtUsuario.Text};Password={txtContrasena.Text};Connection Timeout=30;";
                }

                // Probar la conexión antes de guardar
                if (!await ProbarConexion(nuevaConexion))
                {
                    MessageBox.Show("No se pudo conectar a la base de datos con los parámetros proporcionados. Verifique la configuración.",
                        "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Si llegamos aquí, la conexión es válida, proceder a guardar
                await GuardarConfiguracion(nuevaConexion, providerName);

                MessageBox.Show("Configuración guardada y aplicada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar configuración: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Rehabilitar el botón
                btnGuardar.Enabled = true;
                btnGuardar.Text = "Guardar";
            }
        }

        private async Task<bool> ProbarConexion(string connectionString)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al probar conexión: {ex.Message}");
                return false;
            }
        }

        private async Task GuardarConfiguracion(string nuevaConexion, string providerName)
        {
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

            // Guardar URL de SignalR
            if (config.AppSettings.Settings["SignalRServerUrl"] != null)
            {
                config.AppSettings.Settings["SignalRServerUrl"].Value = txtURLSignal.Text;
            }
            else
            {
                config.AppSettings.Settings.Add("SignalRServerUrl", txtURLSignal.Text);
            }

            // Guardar cambios
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
            ConfigurationManager.RefreshSection("appSettings");

            // IMPORTANTE: Recargar datos y reinicializar SignalR
            main.CargarDatos();

            // Llamar al método para reinicializar SignalR con la nueva configuración
            await main.ReinicializarSignalR();
        }
    }
}