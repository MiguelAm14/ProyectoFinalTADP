using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class formMain: Form
    {
        HubConnection connection;
        IHubProxy hubProxy;
        private bool isReconnecting = false;

        public formMain()
        {
            InitializeComponent();
            InicializarSignalR();

        }
        private async void InicializarSignalR()
        {
            try
            {
                connection = new HubConnection("http://IP_O_DOMINIO_DEL_SERVIDOR:PUERTO/");
                hubProxy = connection.CreateHubProxy("NotificacionHub");
                connection.Closed += async () =>
                {
                    if (!isReconnecting)
                    {
                        isReconnecting = true;
                        await Task.Delay(5000); // Esperar 5 segundos
                        try
                        {
                            await connection.Start();
                            this.Invoke((Action)(() =>
                            {
                                // Actualizar estado de conexión en UI si tienes un label
                                // lblEstado.Text = "Conectado";
                            }));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reconectando: {ex.Message}");
                        }
                        finally
                        {
                            isReconnecting = false;
                        }
                    }
                };

                hubProxy.On("ActualizarDatos", () =>
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke((Action)(() =>
                        {
                            CargarDatos(); // Método que recarga el DataGridView
                        }));
                    }
                    else
                    {
                        CargarDatos();
                    }
                });
                await connection.Start();

                // Opcional: mostrar estado de conexión
                // lblEstado.Text = "Conectado";

            }
            catch(Exception ex)
            {
                MessageBox.Show("Error SignalR: " + ex.Message);
                // lblEstado.Text = "Desconectado";
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

        // Carga los datos de la base de datos y los muestra en el DataGridView
        public void CargarDatos()
        {
            Datos datos = new Datos();
            DataSet ds = datos.ConsultarDS("SELECT matricula AS [Matricula], nombre AS [Nombre], ap_paterno AS [Ap. Paterno]," +
                "ap_materno AS [Ap. Materno], edad AS [Edad], grado as [Grado], seccion as [Sección], tutor AS [Tutor] FROM alumnos");
            dgvAlumnos.DataSource = ds.Tables[0];
            diseno();
            modificar();   
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CargarDatos();
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
        private void dgvAlumnos_CellContentClick(object sender, DataGridViewCellEventArgs e)
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
                formModificar formModificar = new formModificar(this, matricula, nombre, apPaterno, apMaterno, edad, grado, seccion, tutor);
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
                    CargarDatos();
                }
            }
        }

        private void btnInsertar_Click(object sender, EventArgs e)
        {
            // Abrir el form de insertar
            formInsertar formInsertar = new formInsertar(this);
            formInsertar.ShowDialog();
        }

        private void formMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                connection?.Stop();
                connection?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cerrando conexión: {ex.Message}");
            }
        }
    }
}
