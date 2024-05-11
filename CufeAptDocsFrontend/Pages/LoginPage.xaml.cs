using System.Windows;
using System.Windows.Controls;

namespace MRK
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            var username = textboxUsername.Text.Trim();
            if (!Utils.ValidateUsername(username))
            {
                MessageBox.Show("Invalid username");
                return;
            }

            var pwd = textboxPwd.Password.Trim();
            if (!Utils.ValidatePassword(pwd))
            {
                MessageBox.Show("Invalid password");
                return;
            }


            MainWindow.Instance!.ShowMessagePopup("Logging in...");

            var success = await Client.Instance.Login(username, pwd);

            MainWindow.Instance!.HideMessagePopup();

            if (!success)
            {
                MessageBox.Show("Login Failed");
                return;
            }

            // go to homepage
            MainWindow.Instance!.GoTo("Pages/HomePage");
        }

        private void OnRegisterClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance!.GoTo("Pages/RegisterPage");
        }
    }
}