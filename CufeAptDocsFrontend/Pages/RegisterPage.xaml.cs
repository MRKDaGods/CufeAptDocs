using System.Windows;
using System.Windows.Controls;

namespace MRK
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void OnRegisterClick(object sender, RoutedEventArgs e)
        {
            var username = textboxUsername.Text.Trim();
            if (!Utils.ValidateUsername(username))
            {
                MessageBox.Show("Invalid username");
                return;
            }

            var email = textboxEmail.Text.Trim();
            if (!Utils.ValidateEmail(email))
            {
                MessageBox.Show("Invalid email");
                return;
            }

            var pwd = textboxPwd.Password.Trim();
            if (textboxPwd.Password != textboxConfirmPwd.Password || !Utils.ValidatePassword(pwd))
            {
                MessageBox.Show("Invalid password");
                return;
            }

            MainWindow.Instance!.ShowMessagePopup("Creating account...");

            var success = await Client.Instance.Register(
                username,
                email,
                pwd);

            MainWindow.Instance!.HideMessagePopup();

            if (!success)
            {
                MessageBox.Show("Registration Failed");
                return;
            }

            MessageBox.Show("Registration Success!");

            // go to homepage
            MainWindow.Instance!.GoTo("Pages/LoginPage");
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.GoTo("Pages/LoginPage");
        }
    }
}
