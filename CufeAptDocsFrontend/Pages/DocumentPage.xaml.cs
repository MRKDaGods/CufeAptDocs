using MRK.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MRK
{
    /// <summary>
    /// Interaction logic for DocumentPage.xaml
    /// </summary>
    public partial class DocumentPage : Page
    {
        private readonly string _role;

        private bool ReadOnly { get; init; }
        private Document Document { get; init; }

        public DocumentPage(Document document, bool readOnly)
        {
            InitializeComponent();

            Document = document;
            ReadOnly = readOnly;

            labelDocTitle.Content = document.Name;

            _role = readOnly ? "Viewer" : document.OwnerId == Client.Instance.CurrentSession.User.Id ? "Owner" : "Editor";

            labelRole.Content = _role;

            textboxServerIp.KeyDown += OnServerIPKeyDown;

            webView.PreviewKeyDown += WebView_PreviewKeyDown;
        }

        private async void OnServerIPKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                await webView.EnsureCoreWebView2Async();

                webView.CoreWebView2.Navigate($"https://cufe-apt-docs-proxy-frontend.vercel.app/edit/{Document.Id}/{_role}/{Client.Instance.CurrentSession.User.Username}/{Uri.EscapeDataString(textboxServerIp.Text)}");
            }
        }

        private void WebView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ReadOnly)
            {
                e.Handled = true;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance!.GoTo("Pages/HomePage");
        }
    }
}
