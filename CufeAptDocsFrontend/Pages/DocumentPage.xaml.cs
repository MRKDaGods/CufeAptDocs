using MRK.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace MRK
{
    /// <summary>
    /// Interaction logic for DocumentPage.xaml
    /// </summary>
    public partial class DocumentPage : Page
    {
        class DocChar(char c, bool bold, bool italic)
        {
            public char Char { get; set; } = c;
            public bool Bold { get; set; } = bold;
            public bool Italic { get; set; } = italic;
        }

        private readonly Dictionary<string, bool> _documentOptions;
        private readonly List<DocChar> _chars = [];

        private Document Document { get; init; }

        public DocumentPage(Document document)
        {
            InitializeComponent();

            Document = document;

            // init options
            _documentOptions = new Dictionary<string, bool>();
            InitDocumentOptions();

            labelDocTitle.Content = document.Name;
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

            switch (opt)
            {
                case "opt-bold":
                    if (!SetSelectedTextBold(val))
                    {
                        SetCaretBold(val);
                    }

                    break;

                case "opt-italic":
                    if (!SetSelectedTextItalic(val))
                    {
                        SetCaretItalic(val);
                    }
                    break;
            }

            // focus tb again
            richTextboxMain.Focus();
        }

        private bool SetSelectedTextBold(bool set)
        {
            var selection = richTextboxMain.Selection;
            if (selection.IsEmpty)
            {
                return false;
            }

            selection.ApplyPropertyValue(TextElement.FontWeightProperty, set ? FontWeights.Bold : FontWeights.Normal);
            return true;
        }
        
        private void SetCaretBold(bool set)
        {
        }

        private bool SetSelectedTextItalic(bool set)
        {
            var selection = richTextboxMain.Selection;
            if (selection.IsEmpty)
            {
                return false;
            }

            selection.ApplyPropertyValue(TextElement.FontStyleProperty, set ? FontStyles.Italic : FontStyles.Normal);
            return true;
        }

        private void SetCaretItalic(bool set)
        {
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance!.GoTo("Pages/HomePage");
        }
    }
}
