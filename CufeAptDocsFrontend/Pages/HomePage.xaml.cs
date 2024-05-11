using MRK.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MRK
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private object? _lastDocumentOptionsSender;

        private Session Session
        {
            get => Client.Instance.CurrentSession!;
        }

        public HomePage()
        {
            InitializeComponent();
        }

        private void OnHomeLoaded(object sender, RoutedEventArgs e)
        {
            labelUserSettings.Content = labelUsername.Content = Session.User.Username;
            labelEmail.Content = Session.User.Email;
        }

        private void OnUserSettingsLabelClick(object sender, MouseButtonEventArgs e)
        {
            popupLogout.IsOpen = !popupLogout.IsOpen;

            if (popupLogout.IsOpen)
            {
                // position it
                PositionPopup(popupLogout, labelUserSettings);
            }
        }

        private async void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            popupLogout.IsOpen = false;

            MainWindow.Instance!.ShowMessagePopup("Logging out...");

            // notify server
            await Client.Instance.Logout();

            MainWindow.Instance!.HideMessagePopup();

            // go back
            MainWindow.Instance!.GoTo("Pages/LoginPage");
        }

        private void OnDocumentOptionsClick(object sender, RoutedEventArgs e)
        {
            popupDocOptions.IsOpen = sender == _lastDocumentOptionsSender ? !popupDocOptions.IsOpen : true;

            _lastDocumentOptionsSender = sender;

            if (popupDocOptions.IsOpen)
            {
                // position it
                PositionPopup(popupDocOptions, (ContentControl)sender, false);
            }
        }

        private void PositionPopup(Popup popup, ContentControl target, bool inside = true)
        {
            var position = target.TranslatePoint(new Point(0, 0), this);
            
            popup.HorizontalOffset = position.X + target.ActualWidth;
            if (inside)
            {
                popup.HorizontalOffset -= popup.Child.RenderSize.Width;
            }

            popup.VerticalOffset = position.Y + target.ActualHeight;
        }
    }
}
