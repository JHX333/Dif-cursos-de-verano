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
    public partial class Registro : Form
    {
        private const string connStr = "Server=JARED\\DIF;Database=usersdb;Integrated Security=True;";
        private Login regForm = null;
        static class PasswordHelper
        {
            public static byte[] GenerateSalt(int size = 16)
            {
                var rng = new RNGCryptoServiceProvider();
                var salt = new byte[size];
                rng.GetBytes(salt);
                return salt;
            }

            public static string HashPassword(string password, byte[] salt)
            {
                var sha = SHA256.Create();
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

        public Registro()
        {
            InitializeComponent();

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true; // si también quieres ocultar minimizar


        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;

            if (regForm == null || regForm.IsDisposed)
            {
                // Crea solo una instancia si no existe
                regForm = new Login();
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
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox3.Text) ||
                string.IsNullOrWhiteSpace(textBox4.Text) ||
                string.IsNullOrWhiteSpace(textBox5.Text))
            {
                MessageBox.Show("Todos los campos son obligatorios.");
                return;
            }

            var salt = PasswordHelper.GenerateSalt();
            var hash = PasswordHelper.HashPassword(textBox5.Text, salt);

            var cn = new SqlConnection(connStr);
            var cmd = new SqlCommand(
                "INSERT INTO Users (FirstName, LastName, Username, PasswordHash, Email) " +
                "VALUES (@fn,@ln,@usr,@hash,@em)", cn);
            cmd.Parameters.AddWithValue("@fn", textBox1.Text.Trim());
            cmd.Parameters.AddWithValue("@ln", textBox2.Text.Trim());
            cmd.Parameters.AddWithValue("@usr", textBox4.Text.Trim());
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@em", textBox3.Text.Trim());

            cn.Open();
            try
            {
                cmd.ExecuteNonQuery();
                MessageBox.Show("Ha sido registrado con éxito");
                var login = new Login();
                login.Show();
                this.Close();
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("El nombre de usuario ya existe.");
            }
        }
        private void label8_Click(object sender, EventArgs e)
        {
          
        }
    }
}

   