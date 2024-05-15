using MRK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MRK
{
    /// <summary>
    /// Interaction logic for ManageAccessWindow.xaml
    /// </summary>
    public partial class ManageAccessWindow : Window
    {
        private User? _selectedNewUser;
        private bool _ignoreNextTextEvent;

        private List<DocumentUser> DocumentUsers {  get; set; }
        private List<User> UserSuggestions { get; set; }
        private Document Document { get; init; }

        public ManageAccessWindow(Document document)
        {
            InitializeComponent();

            Document = document;

            // fetch stuff
            DocumentUsers = [];
            UserSuggestions = [];

            OnRefreshClick(null, null);
        }

        private async void OnRefreshClick(object? sender, RoutedEventArgs? e)
        {
            var users = await Client.Instance.GetDocumentUsers(Document);
            DocumentUsers = users
                .Select(x => new DocumentUser(x.Key, Document, x.Value))
                .ToList();

            // update item source
            itemsControlUsers.ItemsSource = DocumentUsers;
        }

        private async void OnAddClick(object sender, RoutedEventArgs e)
        {
            if (_selectedNewUser == null) return;

            IsEnabled = false;

            var res = await Client.Instance.AddUserToDocument(_selectedNewUser, Document, comboboxNewPerms.SelectedIndex == 1);

            if (!res)
            {
                MessageBox.Show("Cant add user");
            }
            else
            {
                // refresh
                OnRefreshClick(null, null);
            }

            IsEnabled = true;
        }

        private async void OnNewUserTextChanged(object sender, TextChangedEventArgs e)
        {
            // query
            if (_ignoreNextTextEvent)
            {
                _ignoreNextTextEvent = false;
                return;
            }

            _selectedNewUser = null;

            UserSuggestions = (await Client.Instance.SearchUsers(textboxNewUser.Text))
                .Where(x => DocumentUsers.Find(y => y.User.Id == x.Id) == null)
                .ToList();

            itemsControlUserSuggestions.ItemsSource = UserSuggestions;

            popupUserSuggestions.IsOpen = true;
        }

        private void OnSuggestionClick(object sender, RoutedEventArgs e)
        {
            popupUserSuggestions.IsOpen = false;

            _selectedNewUser = (sender as Button)?.Tag as User;

            _ignoreNextTextEvent = true;
            textboxNewUser.Text = _selectedNewUser?.Username;
        }

        private void OnPopupClose(object sender, RoutedEventArgs e)
        {
            popupUserSuggestions.IsOpen = false;
        }

        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button)?.Tag as User;
            if (user == null) 
            {
                return;
            }

            IsEnabled = false;

            var res = await Client.Instance.DeleteUserFromDocument(user, Document);
            if (!res)
            {
                MessageBox.Show("Cant delete user");
            }
            else
            {
                // refresh
                OnRefreshClick(null, null);
            }

            IsEnabled = true;
        }

        private async void OnPermChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            var cb = sender as ComboBox;
            var docUser = cb?.Tag as DocumentUser;
            if (docUser == null)
            {
                return;
            }

            var canEdit = cb!.SelectedIndex == 1;
            if (docUser.HasEditPermission == canEdit) return;

            IsEnabled = false;

            var res = await Client.Instance.ModifyUserInDocument(docUser.User, Document, canEdit);

            if (!res)
            {
                MessageBox.Show("Cant modify user permissions");
            }
            else
            {
                // refresh
                OnRefreshClick(null, null);
            }

            IsEnabled = true;
        }
    }
}
