using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoginApp
{
    public partial class Form1 : Form
    {

        private const int SALTSIZE = 8;
        private const int NUMBER_OF_ITERATIONS = 1000;

        private readonly string connectionString;

        public Form1()
        {
            InitializeComponent();

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(basePath, @"..\..\DataBase1.mdf");
            dbPath = Path.GetFullPath(dbPath);

            connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='{dbPath}';Integrated Security=True";

        }

        private bool VerifyLogin(string username, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT SaltedPasswordHash, Salt FROM Users WHERE Username = @username";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                throw new Exception("User not found");
                            }

                            reader.Read();
                            byte[] saltedPasswordHashStored = (byte[])reader["SaltedPasswordHash"];
                            byte[] saltStored = (byte[])reader["Salt"];
                            byte[] hash = GenerateSaltedHash(password, saltStored);

                            return saltedPasswordHashStored.SequenceEqual(hash);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred: " + e.Message);
                return false;
            }
        }


        private static byte[] GenerateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return buff;

            /*
             *     using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
             *     {
             *         rng.GetBytes(buff)
             *     }
             */

        }

        private static byte[] GenerateSaltedHash(string plainText, byte[] salt)
        {
            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(plainText, salt, NUMBER_OF_ITERATIONS);
            return rfc2898.GetBytes(32);
        }


        private void Register(string username, byte[] saltedPasswordHash, byte[] salt)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                INSERT INTO Users (Username, SaltedPasswordHash, Salt) 
                VALUES (@username, @saltedPasswordHash, @salt)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@saltedPasswordHash", saltedPasswordHash);
                        cmd.Parameters.AddWithValue("@salt", salt);

                        int lines = cmd.ExecuteNonQuery();

                        if (lines == 0)
                        {
                            throw new Exception("No rows affected. Registration failed.");
                        }

                        MessageBox.Show("Utilizador registado com sucesso!");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error during registration: " + e.Message);
            }
        }





        private void buttonGenerateSaltedHash_Click(object sender, EventArgs e)
        {
            String password = textBoxPassword.Text;

            byte[] salt = GenerateSalt(SALTSIZE);
            byte[] hash = GenerateSaltedHash(password, salt);

            textBoxSaltedHash.Text = Convert.ToBase64String(hash);

            textBoxSalt.Text = Convert.ToBase64String(salt);

            textBoxSizePass.Text = (hash.Length * 8).ToString();
            textBoxSizeSalt.Text = (salt.Length * 8).ToString();

        }

        private void ButtonRegister_Click(object sender, EventArgs e)
        {
            String pass = textBoxPassword.Text;
            String username = textBoxUsername.Text;

            byte[] salt = GenerateSalt(SALTSIZE);
            byte[] hash = GenerateSaltedHash(pass, salt);

            Register(username, hash, salt);


        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            string password = textBoxPassword.Text;
            string username = textBoxUsername.Text;

            if (VerifyLogin(username, password))
            {
                MessageBox.Show("O utilizador é válido!!");

                // Abre o formulário do chat, passando o nome do utilizador autenticado
                ProtocolSI_cliente.Form1 chatForm = new ProtocolSI_cliente.Form1(username);
                chatForm.Show();

                this.Hide(); // Esconde o formulário de login
            }
            else
            {
                MessageBox.Show("Autenticação errada!!");
            }
        }



        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

    }
}
