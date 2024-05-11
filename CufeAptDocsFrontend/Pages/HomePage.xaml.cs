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

        private List<Document> Documents { get; set; }

        private Session Session
        {
            get => Client.Instance.CurrentSession!;
        }

        public HomePage()
        {
            InitializeComponent();

            Documents = new();
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
            popupDocOptions.IsOpen = sender != _lastDocumentOptionsSender || !popupDocOptions.IsOpen;

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

        private void OnDocumentClick(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Instance!.GoTo("Pages/DocumentPage");
        }

        private async void OnCreateDocumentClick(object sender, RoutedEventArgs e)
        {
            // wellll
            MainWindow.Instance!.ShowMessagePopup("Creating...");

            var success = await Client.Instance.CreateDocument("New Document " + new Random().Next());

            MainWindow.Instance!.HideMessagePopup();

            if (!success)
            {
                MessageBox.Show("Cannot create new document");
                return;
            }

            OnRefreshDocumentsClick(sender, e);
        }

        private async void OnRefreshDocumentsClick(object? sender, RoutedEventArgs? e)
        {
            MainWindow.Instance!.ShowMessagePopup("Fetching Documents...");

            Documents = await Client.Instance.GetDocuments();

            MainWindow.Instance!.HideMessagePopup();
        }
    }
}
