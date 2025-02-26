using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace PasswordManager
{
    public partial class MainWindow : Window
    {
        private const string FilePath = "passwords.txt";

        public MainWindow()
        {
            InitializeComponent();
            LoadPasswords();
        }

        private void LoadPasswords()
        {
            if (File.Exists(FilePath))
            {
                var lines = File.ReadAllLines(FilePath);
                var passwordList = new List<PasswordRecord>();

                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    if (parts.Length == 3)
                    {
                        passwordList.Add(new PasswordRecord
                        {
                            Site = parts[0],
                            Login = parts[1],
                            Password = DecryptPassword(parts[2])
                        });
                    }
                }

                passwordListView.ItemsSource = passwordList;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var site = siteTextBox.Text;
            var login = loginTextBox.Text;
            var password = passwordTextBox.Password;

            if (string.IsNullOrEmpty(site) || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Все поля должны быть заполнены!");
                return;
            }

            var encryptedPassword = EncryptPassword(password);

            File.AppendAllText(FilePath, $"{site};{login};{encryptedPassword}\n");

            LoadPasswords();
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            int length = 12;

            var password = GeneratePassword(length);
            passwordTextBox.Password = password;
        }

        private string GeneratePassword(int length)
        {
            var random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var password = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }

            return password.ToString();
        }

        private string EncryptPassword(string password)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("1234567890123456");
                aes.IV = Encoding.UTF8.GetBytes("1234567890123456");

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    var buffer = Encoding.UTF8.GetBytes(password);
                    return Convert.ToBase64String(encryptor.TransformFinalBlock(buffer, 0, buffer.Length));
                }
            }
        }

        private string DecryptPassword(string encryptedPassword)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("1234567890123456");
                aes.IV = Encoding.UTF8.GetBytes("1234567890123456");

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    var buffer = Convert.FromBase64String(encryptedPassword);
                    return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(buffer, 0, buffer.Length));
                }
            }
        }
    }
}