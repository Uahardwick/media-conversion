namespace MediaSuite.Tools.MergePdf;

internal enum MergePosition
{
    Front,
    Back,
}

internal sealed class MergeOptionsForm : Form
{
    private readonly string _clickedPdfPath;
    private readonly TextBox _otherFileTextBox;
    private readonly RadioButton _frontRadioButton;
    private readonly RadioButton _backRadioButton;
    private readonly Label _errorLabel;

    public string OtherPdfPath { get; private set; } = string.Empty;

    public MergePosition Position { get; private set; } = MergePosition.Back;

    public MergeOptionsForm(string clickedPdfPath)
    {
        _clickedPdfPath = clickedPdfPath;
        var clickedFileName = Path.GetFileName(clickedPdfPath);

        Text = "Merge PDFs";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(460, 220);

        var promptLabel = new Label
        {
            Text = $"Choose the PDF to append to \"{clickedFileName}\":",
            Left = 16,
            Top = 16,
            Width = 428,
        };

        _otherFileTextBox = new TextBox { Left = 16, Top = 40, Width = 340, ReadOnly = true };
        var browseButton = new Button { Text = "Browse...", Left = 364, Top = 39, Width = 80 };
        browseButton.Click += BrowseButton_Click;

        var positionLabel = new Label { Text = "Append it to the:", Left = 16, Top = 76, Width = 428 };
        _frontRadioButton = new RadioButton { Text = $"Front (before {clickedFileName})", Left = 32, Top = 100, Width = 400, Checked = false };
        _backRadioButton = new RadioButton { Text = $"Back (after {clickedFileName})", Left = 32, Top = 124, Width = 400, Checked = true };

        _errorLabel = new Label
        {
            Left = 16,
            Top = 152,
            Width = 428,
            Height = 32,
            ForeColor = Color.Firebrick,
        };

        var okButton = new Button { Text = "Merge", Left = 268, Top = 188, Width = 84 };
        var cancelButton = new Button { Text = "Cancel", Left = 360, Top = 188, Width = 84, DialogResult = DialogResult.Cancel };

        okButton.Click += OkButton_Click;

        Controls.AddRange(new Control[]
        {
            promptLabel, _otherFileTextBox, browseButton, positionLabel, _frontRadioButton, _backRadioButton, _errorLabel, okButton, cancelButton,
        });

        AcceptButton = okButton;
        CancelButton = cancelButton;
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Choose the PDF to append",
            Filter = "PDF files (*.pdf)|*.pdf",
            InitialDirectory = Path.GetDirectoryName(_clickedPdfPath) ?? string.Empty,
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _otherFileTextBox.Text = dialog.FileName;
            _errorLabel.Text = string.Empty;
        }
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_otherFileTextBox.Text))
        {
            _errorLabel.Text = "Choose a PDF file to append.";
            return;
        }

        if (string.Equals(
                Path.GetFullPath(_otherFileTextBox.Text),
                Path.GetFullPath(_clickedPdfPath),
                StringComparison.OrdinalIgnoreCase))
        {
            _errorLabel.Text = "Choose a different PDF file to append.";
            return;
        }

        OtherPdfPath = _otherFileTextBox.Text;
        Position = _frontRadioButton.Checked ? MergePosition.Front : MergePosition.Back;
        DialogResult = DialogResult.OK;
        Close();
    }
}
