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

        private Document? LastDocumentOptions
        {
            get => (_lastDocumentOptionsSender as Button)?.Tag as Document;
        }

        public HomePage()
        {
            InitializeComponent();

            Documents = [];

            // refresh
            OnRefreshDocumentsClick(null, null);
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

            this.ShowMessagePopup("Logging out...");

            // notify server
            await Client.Instance.Logout();

            this.HideMessagePopup();

            // go back
            this.GoTo("Pages/LoginPage");
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
            // this.GoTo("Pages/DocumentPage");

            MainWindow.Instance?.frameContent.Navigate(
                new DocumentPage((Document)((Border)sender).Tag));
        }

        private async void OnCreateDocumentClick(object sender, RoutedEventArgs e)
        {
            // get name
            var docName = this.GetStringFromUser("Enter New Document Name", "New Document");
            if (docName == null) return;

            // wellll
            this.ShowMessagePopup("Creating...");

            var success = await Client.Instance.CreateDocument(docName);

            this.HideMessagePopup();

            if (!success)
            {
                MessageBox.Show("Cannot create new document");
                return;
            }

            OnRefreshDocumentsClick(sender, e);
        }

        private async void OnRefreshDocumentsClick(object? sender, RoutedEventArgs? e)
        {
            this.ShowMessagePopup("Fetching Documents...");

            Documents = await Client.Instance.GetDocuments();
            Documents.Sort((x, y) => y.ModificationDate.CompareTo(x.ModificationDate));

            this.HideMessagePopup();

            itemsControlDocs.ItemsSource = Documents;
        }

        private async void OnRenameDocumentClick(object sender, RoutedEventArgs e)
        {
            // close regardless
            popupDocOptions.IsOpen = false;

            var doc = LastDocumentOptions;
            if (doc == null) return;

            var newName = this.GetStringFromUser("Rename Document", doc.Name);
            if (newName == null) return;

            // do req
            var result = await Client.Instance.RenameDocument(doc, newName);
            if (!result)
            {
                MessageBox.Show("Cannot rename document!");
                return;
            }

            // refresh
            OnRefreshDocumentsClick(null, null);
        }

        private void OnChangeAccessClick(object sender, RoutedEventArgs e)
        {
            // close regardless
            popupDocOptions.IsOpen = false;

            var doc = LastDocumentOptions;
            if (doc == null) return;

            var wnd = new ManageAccessWindow(doc);
            wnd.ShowDialog();
        }

        private async void OnDeleteDocumentClick(object sender, RoutedEventArgs e)
        {
            // close regardless
            popupDocOptions.IsOpen = false;

            var doc = LastDocumentOptions;
            if (doc == null) return;

            if (MessageBox.Show("Are you sure?", "Delete Document", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            // do req
            var result = await Client.Instance.DeleteDocument(doc);
            if (!result)
            {
                MessageBox.Show("Cannot delete document!");
                return;
            }

            // refresh
            OnRefreshDocumentsClick(null, null);
        }
    }
}
