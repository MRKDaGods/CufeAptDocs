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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MRK.Pages
{
    /// <summary>
    /// Interaction logic for DocumentPage.xaml
    /// </summary>
    public partial class DocumentPage : Page
    {
        private readonly Dictionary<string, bool> _documentOptions;

        public DocumentPage()
        {
            InitializeComponent();

            // init options
            _documentOptions = new Dictionary<string, bool>();
            InitDocumentOptions();
        }

        private void InitDocumentOptions()
        {
            foreach (UIElement child in panelOptions.Children)
            {
                Button? button = child as Button;
                if (button != null && button.Tag is string opt)
                {
                    _documentOptions[opt] = false;

                    // attach handler
                    button.Click += (o, e) => OnOptionClick(button, opt);
                }
            }
        }

        private void OnOptionClick(Button owner, string opt)
        {
            var val = !_documentOptions[opt];
            _documentOptions[opt] = val;

            // update style
            owner.SetResourceReference(StyleProperty, val ? "EnabledOptionStyle" : "DisabledOptionStyle");
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance!.GoTo("Pages/HomePage");
        }
    }
}
