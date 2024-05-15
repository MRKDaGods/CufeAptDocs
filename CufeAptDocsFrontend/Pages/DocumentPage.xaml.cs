using MRK.Models;
using System.Windows;
using System.Windows.Controls;

namespace MRK
{
    /// <summary>
    /// Interaction logic for DocumentPage.xaml
    /// </summary>
    public partial class DocumentPage : Page
    {
        private bool ReadOnly { get; init; }
        private Document Document { get; init; }

        public DocumentPage(Document document, bool readOnly)
        {
            InitializeComponent();

            Document = document;
            ReadOnly = readOnly;

            labelDocTitle.Content = document.Name;

            InitializeWebView2();

            webView.PreviewKeyDown += WebView_PreviewKeyDown;
        }

        private void WebView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (ReadOnly)
            {
                e.Handled = true;
            }
        }

        private async void InitializeWebView2()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.Source = new Uri("http://localhost:3000/");
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
