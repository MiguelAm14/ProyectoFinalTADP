using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class formInsertar : Form
    {
        // Atributo fmMain para acceder a la instancia del formulario principal
        private formMain fmMain;
        private NotificadorCambios _notificador;
        public formInsertar(formMain fmMain, NotificadorCambios notificador)
        {
            InitializeComponent();
            mtbEdad.ValidatingType = typeof(int); // Validar que la entrada sea un número entero
            this.fmMain = fmMain;
            _notificador = notificador;
        }

        private void formInsertar_Load(object sender, EventArgs e)
        {
            // Diseño del botón insertar
            btnInsertar.FlatStyle = FlatStyle.Flat;
            btnInsertar.FlatAppearance.BorderSize = 2;
            btnInsertar.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#FFFFFF");
            btnInsertar.BackColor = ColorTranslator.FromHtml("#0078D7");
            btnInsertar.ForeColor = Color.White;
            btnInsertar.Font = new Font("Roboto", 10, FontStyle.Bold);
            btnInsertar.Cursor = Cursors.Hand;
        }

        private void mtbEdad_TypeValidationCompleted(object sender, TypeValidationEventArgs e)
        {
            // Validar que la edad sea un número entero entre 1 y 99
            if (!e.IsValidInput)
            {
                MessageBox.Show("Edad inválida. Solo se permiten números enteros entre 1 y 99.");
            }
        }

        private async void btnInsertar_Click(object sender, EventArgs e)
        {
            // Validar si hay campos vacios
            if (txtNombre.Text.Trim() == "")
            {
                txtNombre.Focus();
                MessageBox.Show("Por favor, complete el campo Nombre.", "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtApPaterno.Text.Trim() == "")
            {
                txtApPaterno.Focus();
                MessageBox.Show("Por favor, complete el campo Apellido Paterno.", "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (mtbEdad.Text.Trim() == "")
            {
                mtbEdad.Focus();
                MessageBox.Show("Por favor, complete el campo Edad.", "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Insertar datos
            Datos datos = new Datos();
            bool resultado = datos.Comando(
                "INSERT INTO alumnos (nombre, ap_paterno, ap_materno, edad, grado, seccion, tutor) " +
                "VALUES (@nombre, @ap_paterno, @ap_materno, @edad, @grado, @seccion, @tutor)",
                new SqlParameter("@nombre", txtNombre.Text),
                new SqlParameter("@ap_paterno", txtApPaterno.Text),
                new SqlParameter("@ap_materno", string.IsNullOrWhiteSpace(txtApMaterno.Text) ? (object)DBNull.Value : txtApMaterno.Text),
                new SqlParameter("@edad", Convert.ToInt32(mtbEdad.Text)),
                new SqlParameter("@grado", string.IsNullOrWhiteSpace(txtGrado.Text) ? (object)DBNull.Value : txtGrado.Text),
                new SqlParameter("@seccion", string.IsNullOrWhiteSpace(txtSeccion.Text) ? (object)DBNull.Value : txtSeccion.Text),
                new SqlParameter("@tutor", string.IsNullOrWhiteSpace(txtTutor.Text) ? (object)DBNull.Value : txtTutor.Text)
            );

            if (resultado)
            {
                MessageBox.Show("Alumno insertado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Actualizar la tabla 
                if (_notificador != null)
                {
                    await _notificador.NotificarCambio();
                }

            }
            else
            {
                MessageBox.Show("Error al insertar el alumno. Por favor, inténtelo de nuevo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
