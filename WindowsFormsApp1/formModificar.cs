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
using static System.Collections.Specialized.BitVector32;

namespace WindowsFormsApp1
{
    public partial class formModificar: Form
    {
        private string matricula;
        private string nombre;
        private string apPaterno;
        private string apMaterno;
        private string edad;
        private string grado;
        private string seccion;
        private string tutor;
        private formMain fmMain;

        public formModificar(formMain fmMain, string matricula, string nombre, string apPaterno, string apMaterno, 
                             string edad, string grado, string seccion, string tutor)
        {
            InitializeComponent();
            this.matricula = matricula;
            this.nombre = nombre;
            this.apPaterno = apPaterno;
            this.apMaterno = apMaterno;
            this.edad = edad;
            this.grado = grado;
            this.seccion = seccion;
            this.tutor = tutor;
            this.fmMain = fmMain;
            cargarDatos();
        }

        private void cargarDatos()
        {
            txtNombre.Text = nombre;
            txtApPaterno.Text = apPaterno;
            txtApMaterno.Text = apMaterno;
            mtbEdad.Text = edad;
            txtGrado.Text = grado;
            txtSeccion.Text = seccion;
            txtTutor.Text = tutor;
        }

        private void formModificar_Load(object sender, EventArgs e)
        {
            // Diseño del botón modificar
            btnModificar.FlatStyle = FlatStyle.Flat;
            btnModificar.FlatAppearance.BorderSize = 2;
            btnModificar.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#FFFFFF");
            btnModificar.BackColor = ColorTranslator.FromHtml("#0078D7");
            btnModificar.ForeColor = Color.White;
            btnModificar.Font = new Font("Roboto", 10, FontStyle.Bold);
            btnModificar.Cursor = Cursors.Hand;
        }

        private void btnModificar_Click(object sender, EventArgs e)
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

            // Actualizar datos
            Datos datos = new Datos();
            bool resultado = datos.Comando(
                "UPDATE alumnos SET " +
                "nombre = @nombre, " +
                "ap_paterno = @ap_paterno, " +
                "ap_materno = @ap_materno, " +
                "edad = @edad, " +
                "grado = @grado, " +
                "seccion = @seccion, " +
                "tutor = @tutor " +
                "WHERE matricula = @matricula",
                new SqlParameter("@matricula", matricula),
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
                MessageBox.Show("Alumno actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Actualizar la tabla 
                fmMain.CargarDatos();
            }
            else
            {
                MessageBox.Show("Error al actualizar el alumno. Por favor, inténtelo de nuevo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    
    }
}
