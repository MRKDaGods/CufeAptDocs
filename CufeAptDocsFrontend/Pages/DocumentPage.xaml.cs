using MRK.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FTB = FastColoredTextBoxNS;
using System.Drawing;
using WF = System.Windows.Forms;

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

        private FTB.FastColoredTextBox? _textbox;

        private int _styleIndexBold;
        private int _styleIndexItalic;
        private int _styleIndexBoth;

        private FTB.Range? _newTextRange;

        private Document Document { get; init; }
        private FTB.FastColoredTextBox TextBox => _textbox!;

        public DocumentPage(Document document)
        {
            InitializeComponent();

            Document = document;

            // init options
            _documentOptions = new Dictionary<string, bool>();
            InitDocumentOptions();

            labelDocTitle.Content = document.Name;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _textbox = new()
            {
                BackColor = Color.FromArgb(37, 37, 37),
                ForeColor = Color.FromArgb(241, 241, 241),

                Font = new Font("Consolas", 16f, GraphicsUnit.Pixel),

                CaretColor = Color.FromArgb(220, 220, 200),
                Paddings = new WF.Padding(50),

                ShowLineNumbers = false
            };

            _textbox.TextChanged += OnTextChanged;
            _textbox.TextChanging += OnTextChanging;

            // bold
            _styleIndexBold = _textbox.AddStyle(new FTB.TextStyle(null, null, System.Drawing.FontStyle.Bold));

            _styleIndexItalic = _textbox.AddStyle(new FTB.TextStyle(null, null, System.Drawing.FontStyle.Italic));

            _styleIndexBoth = _textbox.AddStyle(new FTB.TextStyle(null, null, System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Bold));

            host.Child = _textbox;
        }

        private void OnTextChanging(object? sender, FTB.TextChangingEventArgs e)
        {
            switch (e.InsertingText)
            {
                case null:
                case "\b":
                case "\n":
                case "\r":
                case "\t":
                    return;
            }

            var start = TextBox.Selection.Start;

            _newTextRange = new FTB.Range(TextBox, start, start + new FTB.Place(e.InsertingText.Length, 0));
        }

        private void OnTextChanged(object? sender, FTB.TextChangedEventArgs e)
        {
            if (_newTextRange != null)
            {
                UpdateRangeStyle(_newTextRange);
                _newTextRange = null;
            }
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

            UpdateRangeStyle(TextBox.Selection);

            // focus tb again
            TextBox.Focus();
        }

        private void UpdateRangeStyle(FTB.Range range)
        {
            var boldStyle = TextBox.Styles[_styleIndexBold];
            var italicStyle = TextBox.Styles[_styleIndexItalic];
            var bothStyle = TextBox.Styles[_styleIndexBoth];

            // bold enabled?
            var bold = _documentOptions["opt-bold"];
            var italic = _documentOptions["opt-italic"];

            range.ClearStyle(boldStyle, italicStyle, bothStyle);

            if (bold && italic)
            {
                range.SetStyle(bothStyle);
            }
            else if (bold)
            {
                range.SetStyle(boldStyle);
            }
            else if (italic)
            {
                range.SetStyle(italicStyle);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance!.GoTo("Pages/HomePage");
        }
    }
}
