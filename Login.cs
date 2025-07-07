using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace Difcursos
{
    public partial class Login : Form
    {
        private const string connStr = "Server=JARED\\DIF;Database=usersdb;Integrated Security=True;";
        private Registro regForm = null;
        static class PasswordHelper
        {
            public static byte[] GenerateSalt(int size = 16)
            {
                using (var rng = RandomNumberGenerator.Create())
                {
                    var salt = new byte[size];
                    rng.GetBytes(salt);
                    return salt;
                }
            }

            public static string HashPassword(string password, byte[] salt)
            {
                var sha = SHA256.Create() ;
                var pwdBytes = Encoding.UTF8.GetBytes(password);
                var all = salt.Concat(pwdBytes).ToArray();
                var hash = sha.ComputeHash(all);

                // Almacena salt + hash en Base64
                return Convert.ToBase64String(salt.Concat(hash).ToArray());
            }

            public static bool VerifyPassword(string enteredPwd, string storedHashBase64)
            {
                var storedBytes = Convert.FromBase64String(storedHashBase64);
                var salt = storedBytes.Take(16).ToArray();
                var hashInDb = storedBytes.Skip(16).ToArray();

                var sha = SHA256.Create();
                var pwdBytes = Encoding.UTF8.GetBytes(enteredPwd);
                var checkHash = sha.ComputeHash(salt.Concat(pwdBytes).ToArray());

                return checkHash.SequenceEqual(hashInDb);

            }
        }

        public Login()
        {
            InitializeComponent();

            // pictureBox3 será hijo de pictureBox1
            pictureBox1.Controls.Add(pictureBox3);
            // ajusta posición relativa e ignora color de fondo
            pictureBox3.Location = new Point(23, 129);
            pictureBox3.BackColor = Color.Transparent;

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true; // si también quieres ocultar minimizar
            linkLabel2.LinkClicked += linkLabel2_LinkClicked;

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {
           
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel2.LinkVisited = true;

            if (regForm == null || regForm.IsDisposed)
            {
                // Crea solo una instancia si no existe
                regForm = new Registro();
                regForm.FormClosed += (s, args) => regForm = null;
                regForm.Show();
            }
            else
            {
                regForm.BringToFront();
            }

            this.Hide(); // Oculta la ventana de login
        }
        
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Ingresa usuario y contraseña.");
                return;
               
            }

            var cn = new SqlConnection(connStr);
            var cmd = new SqlCommand("SELECT PasswordHash FROM Users WHERE Username = @usr", cn);
            cmd.Parameters.AddWithValue("@usr", textBox1.Text.Trim());
            cn.Open();

            var stored = cmd.ExecuteScalar() as string;
            if (stored == null)
            {
                MessageBox.Show("Usuario incorrecto.");
                return;
            }

            if (PasswordHelper.VerifyPassword(textBox2.Text, stored))
            {
                MessageBox.Show("Inicio de sesión exitoso");

                // PASAR USUARIO ACTIVO A FrmPrincipal
                FrmPrincipal frmPrincipal = new FrmPrincipal(textBox1.Text.Trim());
                frmPrincipal.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Contraseña incorrecta.");
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkLabel1.LinkClicked += linkLabel1_LinkClicked;

            string username = Microsoft.VisualBasic.Interaction.InputBox(
        "¿Cuál es el usuario del que desea recuperar la contraseña?", "Recuperar contraseña");

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Operación cancelada.");
                return;
            }

            using (SqlConnection cn = new SqlConnection(connStr))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Email FROM Users WHERE Username = @usr", cn);
                cmd.Parameters.AddWithValue("@usr", username.Trim());
                object result = cmd.ExecuteScalar();

                if (result == null)
                {
                    MessageBox.Show("El usuario no existe.");
                    return;
                }

                string email = result.ToString();
                DialogResult dr = MessageBox.Show($"¿El correo registrado es: {email}?", "Confirmar correo", MessageBoxButtons.YesNo);

                while (dr == DialogResult.No)
                {
                    string nuevoCorreo = Microsoft.VisualBasic.Interaction.InputBox("Ingresa el correo correcto:", "Actualizar correo");
                    if (string.IsNullOrWhiteSpace(nuevoCorreo))
                    {
                        MessageBox.Show("Operación cancelada.");
                        return;
                    }

                    SqlCommand updateCmd = new SqlCommand("UPDATE Users SET Email = @em WHERE Username = @usr", cn);
                    updateCmd.Parameters.AddWithValue("@em", nuevoCorreo.Trim());
                    updateCmd.Parameters.AddWithValue("@usr", username.Trim());
                    updateCmd.ExecuteNonQuery();

                    email = nuevoCorreo.Trim();
                    dr = MessageBox.Show($"¿El correo registrado ahora es: {email}?", "Confirmar correo", MessageBoxButtons.YesNo);
                }

                // Generar nueva contraseña
                string nuevaPwd = GenerarContraseña(8);
                byte[] salt = PasswordHelper.GenerateSalt();
                string hash = PasswordHelper.HashPassword(nuevaPwd, salt);

                SqlCommand updatePwdCmd = new SqlCommand("UPDATE Users SET PasswordHash = @hash WHERE Username = @usr", cn);
                updatePwdCmd.Parameters.AddWithValue("@hash", hash);
                updatePwdCmd.Parameters.AddWithValue("@usr", username.Trim());
                updatePwdCmd.ExecuteNonQuery();

                // Enviar correo
                if (EnviarCorreo(email, nuevaPwd))
                {
                    MessageBox.Show("La nueva contraseña ha sido enviada al correo correctamente.");
                }
                else
                {
                    MessageBox.Show("No se pudo enviar el correo. Contacte a soporte.");
                }
            }
        }
            private string GenerarContraseña(int longitud)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, longitud)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private bool EnviarCorreo(string destino, string nuevaPwd)
        {
            try
            {
                System.Net.Mail.MailMessage correo = new System.Net.Mail.MailMessage();
                correo.From = new System.Net.Mail.MailAddress("tucorreo@dominio.com", "DIF Cursos");
                correo.To.Add(destino);
                correo.Subject = "Recuperación de contraseña DIF Cursos";
                correo.Body = $"Su nueva contraseña es: {nuevaPwd}\nPor favor, cámbiela después de iniciar sesión.";
                correo.IsBodyHtml = false;

                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com");
                smtp.Port = 587;
                smtp.Credentials = new System.Net.NetworkCredential("tucorreo@dominio.com", "tu_contraseña_o_app_password");
                smtp.EnableSsl = true;

                smtp.Send(correo);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar correo: " + ex.Message);
                return false;
            }
        }
        
    }
    }
    
   