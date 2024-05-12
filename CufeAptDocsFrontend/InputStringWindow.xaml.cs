using System.Windows;

namespace MRK
{
    public partial class InputStringWindow : Window
    {
        private readonly Action<string> _callback;

        public InputStringWindow(string header, Action<string> callback, string? oldText = null)
        {
            InitializeComponent();

            textblockHeader.Text = header;

            _callback = callback;

            if (oldText != null)
            {
                textboxMain.Text = oldText;
            }
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            _callback(textboxMain.Text.Trim());

            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }
}
