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

        // OT stuff
        private NetClient client;
        private bool isLocalChange = false;
        private string previousText;

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

            // OT client
            client = new NetClient(ApplyOperation);
            client.Connect("localhost", 23466, "yo");
        }

        private int FindChangeStart(string oldText, string newText)
        {
            int minLength = Math.Min(oldText.Length, newText.Length);
            for (int i = 0; i < minLength; i++)
            {
                if (oldText[i] != newText[i])
                {
                    return i;
                }
            }
            return minLength;
        }

        private int FindChangeEnd(string oldText, string newText, int start)
        {
            int oldLength = oldText.Length;
            int newLength = newText.Length;
            int minLength = Math.Min(oldLength, newLength);

            for (int i = 0; i < minLength - start; i++)
            {
                if (oldText[oldLength - 1 - i] != newText[newLength - 1 - i])
                {
                    return newLength - i;
                }
            }
            return newLength;
        }

        private void ApplyOperation(Operation operation)
        {
            isLocalChange = true;

            if (operation is InsertOperation insertOperation)
            {
                var place = TextBox.PositionToPlace(insertOperation.Position);
                TextBox.Selection.Start = place;
                TextBox.InsertText(insertOperation.Text);
            }
            else if (operation is DeleteOperation deleteOperation)
            {
                var start = TextBox.PositionToPlace(deleteOperation.Position);
                var end = TextBox.PositionToPlace(deleteOperation.Position + deleteOperation.Length);
                TextBox.Selection.Start = start;
                TextBox.Selection.End = end;
                TextBox.ClearSelected();
            }

            isLocalChange = false;
            previousText = TextBox.Text;
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

            previousText = TextBox.Text;
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

            if (!isLocalChange && client != null && client.IsConnected)
            {
                var currentText = TextBox.Text;
                var changeStart = FindChangeStart(previousText, currentText);
                var changeEnd = FindChangeEnd(previousText, currentText, changeStart);

                if (changeStart < changeEnd)
                {
                    // Text was added
                    var insertedText = currentText.Substring(changeStart, changeEnd - changeStart);
                    var insertOperation = new InsertOperation(changeStart, insertedText, client.UserId);
                    client.SendOperation(insertOperation);
                }
                else if (changeStart > changeEnd)
                {
                    // Text was removed
                    var deleteLength = previousText.Length - currentText.Length;
                    var deleteOperation = new DeleteOperation(changeStart, deleteLength, client.UserId);
                    client.SendOperation(deleteOperation);
                }

                previousText = currentText;
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
