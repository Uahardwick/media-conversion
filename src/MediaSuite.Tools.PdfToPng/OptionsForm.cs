using MediaSuite.Core.FileSystem;
using MediaSuite.Core.Pdf;

namespace MediaSuite.Tools.PdfToPng;

internal sealed class OptionsForm : Form
{
    private readonly TextBox _nameTextBox;
    private readonly TextBox _pagesTextBox;
    private readonly Label _errorLabel;
    private readonly int _totalPages;

    public string OutputName { get; private set; } = string.Empty;

    public IReadOnlyList<int> Pages { get; private set; } = Array.Empty<int>();

    public OptionsForm(string defaultName, int totalPages)
    {
        _totalPages = totalPages;

        Text = "Convert PDF to PNG";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(420, 220);

        var nameLabel = new Label { Text = "Output name:", Left = 16, Top = 16, Width = 388 };
        _nameTextBox = new TextBox { Left = 16, Top = 40, Width = 388, Text = defaultName, MaxLength = 100 };

        var pagesLabel = new Label
        {
            Text = $"Pages to export (this PDF has {totalPages} page{(totalPages == 1 ? string.Empty : "s")}):",
            Left = 16,
            Top = 76,
            Width = 388,
        };
        _pagesTextBox = new TextBox { Left = 16, Top = 100, Width = 388, Text = "All" };

        var hintLabel = new Label
        {
            Text = "Examples: All  |  3  |  1, 2-5",
            Left = 16,
            Top = 128,
            Width = 388,
            ForeColor = SystemColors.GrayText,
        };

        _errorLabel = new Label
        {
            Left = 16,
            Top = 152,
            Width = 388,
            Height = 32,
            ForeColor = Color.Firebrick,
        };

        var okButton = new Button { Text = "Convert", Left = 228, Top = 188, Width = 84 };
        var cancelButton = new Button { Text = "Cancel", Left = 320, Top = 188, Width = 84, DialogResult = DialogResult.Cancel };

        okButton.Click += OkButton_Click;

        Controls.AddRange(new Control[]
        {
            nameLabel, _nameTextBox, pagesLabel, _pagesTextBox, hintLabel, _errorLabel, okButton, cancelButton,
        });

        AcceptButton = okButton;
        CancelButton = cancelButton;
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        var nameResult = FileNameValidator.Validate(_nameTextBox.Text.Trim(), maxLength: 100);
        if (!nameResult.IsValid)
        {
            _errorLabel.Text = nameResult.ErrorMessage;
            return;
        }

        var pagesResult = PageRangeParser.Parse(_pagesTextBox.Text, _totalPages);
        if (!pagesResult.IsValid)
        {
            _errorLabel.Text = pagesResult.ErrorMessage;
            return;
        }

        OutputName = _nameTextBox.Text.Trim();
        Pages = pagesResult.Pages;
        DialogResult = DialogResult.OK;
        Close();
    }
}
