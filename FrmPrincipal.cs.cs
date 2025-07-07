using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Difcursos
{

    public partial class FrmPrincipal : Form
    {
        private string usuarioActivo;
        private SqlDataAdapter da;
        private DataTable dt;
        private readonly string connString = ("Server=JARED\\DIF;Database=MiTabla;Integrated Security=True;");
        public FrmPrincipal(string usuario)
        {
            InitializeComponent();
            this.usuarioActivo = usuario;
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            CargarRegistros();
        }
        private void CargarRegistros()
        {
            using (var conn = new SqlConnection(connString))
            {
                var sql = @"SELECT Id, Usuarios, Profesores, Cursos, Alumnos,
                            Inscritos, Constancias, Pagos,
                            FechaCreacion, FechaModificacion
                     FROM RegistrosUsuarios
                     WHERE Usuario = @usuario
                     ORDER BY FechaCreacion";

                da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@usuario", usuarioActivo);
                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                dt = new DataTable();
                da.Fill(dt);
                dt.AcceptChanges();

                // DefaultValue para nuevas filas
                var col = dt.Columns["FechaCreacion"];
                col.AllowDBNull = false;
                col.DefaultValue = DateTime.Now;

                // Limpiar DataGridView antes de asignar
                dataGridView1.DataSource = null;
                dataGridView1.Columns.Clear();

                // Asignar nuevo DataSource
                dataGridView1.DataSource = dt;

                // Ocultar Id
                if (dataGridView1.Columns.Contains("Id"))
                    dataGridView1.Columns["Id"].Visible = false;

                // Configuración del grid
                dataGridView1.ReadOnly = false;
                dataGridView1.AllowUserToAddRows = true;
                dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;

                // Eventos
                dataGridView1.DefaultValuesNeeded -= dataGridView1_DefaultValuesNeeded;
                dataGridView1.DefaultValuesNeeded += dataGridView1_DefaultValuesNeeded;
                dataGridView1.DataError -= dataGridView1_DataError;
                dataGridView1.DataError += dataGridView1_DataError;
            }
        }

        private void dataGridView1_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["FechaCreacion"].Value = DateTime.Now;
            e.Row.Cells["FechaModificacion"].Value = DateTime.Now;
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            var dgv = (DataGridView)sender;
            dgv.Rows[e.RowIndex].ErrorText = "Falta FechaCreacion o formato inválido";
        }


        private void FrmPrincipal_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();

            this.FormClosed += FrmPrincipal_FormClosed;
        }
        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("¿Estás seguro de cerrar sesión?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                this.Hide();
                Login loginForm = new Login();
                loginForm.Show();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void btnGuardarGrid_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para guardar.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue;

                        var drv = row.DataBoundItem as DataRowView;
                        if (drv == null) continue;

                        if (drv.Row.RowState == DataRowState.Added)
                        {
                            using (SqlCommand insertCmd = new SqlCommand(
                                @"INSERT INTO RegistrosUsuarios 
                        (Usuarios, Profesores, Cursos, Alumnos, Inscritos, Constancias, Pagos, FechaCreacion, FechaModificacion, Usuario)
                        VALUES 
                        (@usuarios, @profesores, @cursos, @alumnos, @inscritos, @constancias, @pagos, @fechaCreacion, @fechaModificacion, @usuario)", conn))
                            {
                                insertCmd.Parameters.AddWithValue("@usuarios", row.Cells["Usuarios"].Value ?? DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@profesores", row.Cells["Profesores"].Value ?? DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@cursos", row.Cells["Cursos"].Value ?? DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@alumnos", row.Cells["Alumnos"].Value ?? DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@inscritos", row.Cells["Inscritos"].Value ?? DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@constancias", row.Cells["Constancias"].Value ?? DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@pagos", row.Cells["Pagos"].Value ?? DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@fechaCreacion", DateTime.Now);
                                insertCmd.Parameters.AddWithValue("@fechaModificacion", DateTime.Now);
                                insertCmd.Parameters.AddWithValue("@usuario", usuarioActivo);

                                insertCmd.ExecuteNonQuery();
                            }
                        }
                        else if (drv.Row.RowState == DataRowState.Modified)
                        {
                            int id = Convert.ToInt32(drv.Row["Id"]);
                            using (SqlCommand cmd = new SqlCommand(
                                    "UPDATE RegistrosUsuarios SET Usuarios=@usuarios, Profesores=@profesores, " +
                                    "Cursos=@cursos, Alumnos=@alumnos, Inscritos=@inscritos, Constancias=@constancias, " +
                                    "Pagos=@pagos, FechaModificacion=@fechaModificacion WHERE Id=@id", conn))
                            {
                                cmd.Parameters.AddWithValue("@usuarios", row.Cells["Usuarios"].Value ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@profesores", row.Cells["Profesores"].Value ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@cursos", row.Cells["Cursos"].Value ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@alumnos", row.Cells["Alumnos"].Value ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@inscritos", row.Cells["Inscritos"].Value ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@constancias", row.Cells["Constancias"].Value ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@pagos", row.Cells["Pagos"].Value ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@fechaModificacion", DateTime.Now);
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    MessageBox.Show("Datos guardados correctamente para el usuario: " + usuarioActivo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }

            CargarRegistros();
        }
    }
}