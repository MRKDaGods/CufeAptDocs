using System.Windows;
using System.Windows.Input;

namespace MRK
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }

        public MainWindow()
        {
            Instance = this;

            InitializeComponent();

            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();

            // go to login initially
            GoTo("Pages/LoginPage");
        }

        public void GoTo(string page)
        {
            frameContent.Navigate(new Uri($"{page}.xaml", UriKind.Relative));
        }

        public void ShowMessagePopup(string text)
        {
            Dispatcher.Invoke(() =>
            {
                popupMessageText.Text = text;
                popupMessage.IsOpen = true;
                IsEnabled = false;

                RepositionMessageBox();
            });
        }

        public void HideMessagePopup()
        {
            Dispatcher.Invoke(() =>
            {
                popupMessage.IsOpen = false;
                IsEnabled = true;
            });
        }

        public string? GetStringFromUser(string header, string? oldText = null)
        {
            string? result = null;

            var wnd = new InputStringWindow(header, x => result = x, oldText);
            wnd.ShowDialog();

            return result;
        }

        private void RepositionMessageBox()
        {
            popupMessage.HorizontalOffset = Left + (Width - popupMessage.Child.DesiredSize.Width) / 2;
            popupMessage.VerticalOffset = Top + (Height - popupMessage.Child.DesiredSize.Height) / 2;
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            RepositionMessageBox();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RepositionMessageBox();
        }
    }
}
