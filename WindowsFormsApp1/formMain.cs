using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1;
using Microsoft.Owin.Host.HttpListener;
using System.Windows.Forms.VisualStyles;

namespace WindowsFormsApp1
{
    public partial class formMain : Form
    {
        private NotificadorCambios _notificador;
        private IDisposable _webApp;
        private bool _esServidor = false; // Flag para saber si somos servidor
        private string _currentSignalRUrl = "";

        public formMain()
        {
            InitializeComponent();
        }

        private void MostrarEstado(string mensaje)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(MostrarEstado), mensaje);
                return;
            }

            lblEstado.Text = mensaje;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await InicializarSistema();
        }

        private async Task InicializarSistema()
        {
            try
            {
                // Recargar configuración
                ConfigurationManager.RefreshSection("appSettings");
                ConfigurationManager.RefreshSection("connectionStrings");

                string baseUrl = ConfigurationManager.AppSettings["SignalRServerUrl"];

                // Validar conexión a BD
                Datos datos = new Datos();
                if (!datos.TestConnection())
                {
                    MessageBox.Show("No se pudo conectar a la base de datos. Por favor, configure la conexión.",
                        "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    formConfig form = new formConfig(this);
                    form.ShowDialog();
                    return;
                }

                CargarDatos();
                await InicializarSignalR(baseUrl);
            }
            catch (Exception ex)
            {
                MostrarEstado($"Error al inicializar: {ex.Message}");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR en inicialización: {ex}");
            }
        }

        private async Task InicializarSignalR(string baseUrl)
        {
            try
            {
                // Si ya hay una conexión activa, cerrarla primero
                await CerrarConexionesSignalR();

                _currentSignalRUrl = baseUrl;
                string signalRUrl = baseUrl + "signalr";

                // Intentar iniciar como servidor
                try
                {
                    _webApp = WebApp.Start<WindowsFormsApp1.Inicio>(url: baseUrl);
                    _esServidor = true;
                    Console.WriteLine($"Servidor SignalR iniciado en {baseUrl}");
                    MostrarEstado($"Servidor activo en {baseUrl}");
                }
                catch (Exception ex)
                {
                    _esServidor = false;
                    Console.WriteLine($"No se pudo iniciar servidor: {ex.Message}");
                    MostrarEstado($"Conectando como cliente a {baseUrl}");
                }

                // Inicializar cliente (tanto si somos servidor como cliente)
                await InicializarCliente(signalRUrl);
            }
            catch (Exception ex)
            {
                MostrarEstado($"Error en SignalR: {ex.Message}");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR en SignalR: {ex}");
            }
        }

        private async Task InicializarCliente(string signalRUrl)
        {
            try
            {
                _notificador = new NotificadorCambios(signalRUrl);
                _notificador.OnCambioRecibido += ActualizarGridDatos;

                await _notificador.IniciarConexion();

                string estado = _esServidor ?
                    $"Servidor y Cliente activos en {_currentSignalRUrl}" :
                    $"Cliente conectado a {_currentSignalRUrl}";

                MostrarEstado($"{estado} | Esperando cambios...");
            }
            catch (Exception ex)
            {
                MostrarEstado($"Error al conectar cliente: {ex.Message}");
                throw;
            }
        }

        private async Task CerrarConexionesSignalR()
        {
            try
            {
                // Cerrar cliente
                if (_notificador != null)
                {
                    _notificador.OnCambioRecibido -= ActualizarGridDatos;
                    _notificador.Dispose();
                    _notificador = null;
                }

                // Cerrar servidor
                if (_webApp != null)
                {
                    _webApp.Dispose();
                    _webApp = null;
                    _esServidor = false;
                    Console.WriteLine("Servidor SignalR detenido.");
                }

                // Dar tiempo para que se liberen los recursos
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cerrar conexiones: {ex.Message}");
            }
        }

        // Método público para ser llamado desde formConfig
        public async Task ReinicializarSignalR()
        {
            try
            {
                MostrarEstado("Reinicializando SignalR...");

                // Recargar configuración
                ConfigurationManager.RefreshSection("appSettings");
                string baseUrl = ConfigurationManager.AppSettings["SignalRServerUrl"];

                // Reinicializar SignalR con la nueva configuración
                await InicializarSignalR(baseUrl);

                MostrarEstado("SignalR reinicializado correctamente.");
            }
            catch (Exception ex)
            {
                MostrarEstado($"Error al reinicializar SignalR: {ex.Message}");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR en reinicialización: {ex}");
            }
        }

        private void ActualizarGridDatos()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ActualizarGridDatos));
                return;
            }

            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Actualizando interfaz...");
                CargarDatos();

                string estadoBase = _esServidor ?
                    $"Servidor y Cliente activos en {_currentSignalRUrl}" :
                    $"Cliente conectado a {_currentSignalRUrl}";

                MostrarEstado($"{estadoBase} | Última actualización: {DateTime.Now:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR en ActualizarGridDatos: {ex}");
            }
        }

        private void diseno()
        {
            // Diseño del formulario
            dgvAlumnos.EnableHeadersVisualStyles = false;
            dgvAlumnos.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#0078D7");
            dgvAlumnos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvAlumnos.ColumnHeadersDefaultCellStyle.Font = new Font("Roboto", 10, FontStyle.Bold);
            dgvAlumnos.DefaultCellStyle.Font = new Font("Roboto", 10);
            dgvAlumnos.DefaultCellStyle.BackColor = Color.White;
            dgvAlumnos.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
            dgvAlumnos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAlumnos.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#CDE6F7");
            dgvAlumnos.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Anchos de columna
            dgvAlumnos.Columns["Matricula"].Width = 70;
            dgvAlumnos.Columns["Edad"].Width = 50;
            dgvAlumnos.Columns["Grado"].Width = 52;
            dgvAlumnos.Columns["Sección"].Width = 59;
            dgvAlumnos.Columns["Tutor"].Width = 140;

            // Diseño del botón insertar
            btnInsertar.FlatStyle = FlatStyle.Flat;
            btnInsertar.FlatAppearance.BorderSize = 2;
            btnInsertar.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#FFFFFF");
            btnInsertar.BackColor = Color.White;
            btnInsertar.ForeColor = ColorTranslator.FromHtml("#0078D7");
            btnInsertar.Font = new Font("Roboto", 10, FontStyle.Bold);
            btnInsertar.Cursor = Cursors.Hand;

            // Hover efecto
            btnInsertar.MouseEnter += (s, e) =>
            {
                btnInsertar.BackColor = ColorTranslator.FromHtml("#E6F0FA");
            };
            btnInsertar.MouseLeave += (s, e) =>
            {
                btnInsertar.BackColor = Color.White;
            };
        }

        // Añade botones de eliminar y editar a cada fila del DataGridView
        private void modificar()
        {
            // Eliminar columnas si ya existen
            foreach (DataGridViewColumn col in dgvAlumnos.Columns.Cast<DataGridViewColumn>().ToList())
            {
                if (col.Name == "btnEditar" || col.Name == "btnEliminar")
                    dgvAlumnos.Columns.Remove(col);
            }

            // Agregar botón Editar
            DataGridViewImageColumn btnEditar = new DataGridViewImageColumn();
            btnEditar.Name = "btnEditar";
            btnEditar.HeaderText = "";
            btnEditar.Image = Properties.Resources.lapiz;
            btnEditar.Width = 31;
            btnEditar.ImageLayout = DataGridViewImageCellLayout.Zoom;
            dgvAlumnos.Columns.Add(btnEditar);

            // Agregar botón Eliminar
            DataGridViewImageColumn btnEliminar = new DataGridViewImageColumn();
            btnEliminar.Name = "btnEliminar";
            btnEliminar.HeaderText = "";
            btnEliminar.Image = Properties.Resources.basura;
            btnEliminar.Width = 32;
            btnEliminar.ImageLayout = DataGridViewImageCellLayout.Zoom;
            dgvAlumnos.Columns.Add(btnEliminar);
        }

        public void CargarDatos()
        {
            Datos datos = new Datos();
            DataSet ds = datos.ConsultarDS("SELECT matricula AS [Matricula], nombre AS [Nombre], ap_paterno AS [Ap. Paterno]," +
                "ap_materno AS [Ap. Materno], edad AS [Edad], grado as [Grado], seccion as [Sección], tutor AS [Tutor] FROM alumnos");
            dgvAlumnos.DataSource = ds.Tables[0];
            diseno();
            modificar();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Datos datos = new Datos();

            // Filtrar los elementos según el texto ingresado en el TextBox
            string query = txtBusqueda.Text.Trim();
            if (query.Length > 0)
            {
                DataSet ds = datos.ConsultarDS("SELECT matricula AS [Matricula], nombre AS [Nombre], ap_paterno AS [Ap. Paterno]," +
                    "ap_materno AS [Ap. Materno], edad AS [Edad], grado as [Grado], seccion as [Sección], tutor AS [Tutor] " +
                    "FROM alumnos WHERE nombre LIKE @query OR ap_paterno LIKE @query OR ap_materno LIKE @query",
                    new SqlParameter("@query", "%" + query + "%"));
                dgvAlumnos.DataSource = ds.Tables[0];

                diseno();
                modificar();
            }
            else
            {
                // Recarga los datos originales si la query está vacía
                CargarDatos();
            }
        }

        // Manejador de eventos para los clics en las celdas del DataGridView, ya sea editar o borrar
        private async void dgvAlumnos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return; // Asegurarse de que no se haga clic en el encabezado

            string columna = dgvAlumnos.Columns[e.ColumnIndex].Name;
            DataGridViewRow fila = dgvAlumnos.Rows[e.RowIndex];
            string matricula = fila.Cells["Matricula"].Value.ToString();

            if (columna == "btnEditar")
            {
                // Obtener los datos de la fila seleccionada
                string nombre = fila.Cells["Nombre"].Value.ToString();
                string apPaterno = fila.Cells["Ap. Paterno"].Value.ToString();
                string apMaterno = fila.Cells["Ap. Materno"].Value.ToString();
                string edad = fila.Cells["Edad"].Value.ToString();
                string grado = fila.Cells["Grado"].Value.ToString();
                string seccion = fila.Cells["Sección"].Value.ToString();
                string tutor = fila.Cells["Tutor"].Value.ToString();
                // Mandar al form de edición
                formModificar formModificar = new formModificar(this, _notificador, matricula, nombre, apPaterno, apMaterno, edad, grado, seccion, tutor);
                formModificar.ShowDialog();
            }
            else if (columna == "btnEliminar")
            {
                DialogResult result = MessageBox.Show(
                    $"¿Eliminar al alumno con matrícula {matricula}?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    Datos datos = new Datos();
                    datos.Comando("DELETE FROM alumnos WHERE matricula = @matricula",
                        new SqlParameter("@matricula", matricula));
                    await _notificador.NotificarCambio(); // Notificar el cambio a los clientes conectados
                }
            }
        }

        private void btnInsertar_Click(object sender, EventArgs e)
        {
            if (_notificador != null)
            {
                // **Pasar 'this' (referencia a formMain) y '_notificador' al constructor**
                formInsertar formInsertar = new formInsertar(this, _notificador);
                formInsertar.ShowDialog();
            }
            else
            {
                MessageBox.Show("El sistema de notificación no está inicializado.", "Error de Notificación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void formMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                MostrarEstado("Cerrando conexiones...");
                await CerrarConexionesSignalR();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cerrar: {ex.Message}");
            }

            base.OnFormClosing(e);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            formConfig form = new formConfig(this);
            form.ShowDialog();
        }
    }
}