using MediaSuite.Core.FileSystem;

namespace MediaSuite.Tools.MakePdf;

internal sealed class OptionsForm : Form
{
    private readonly TextBox _nameTextBox;
    private readonly Label _errorLabel;

    public string OutputName { get; private set; } = string.Empty;

    public OptionsForm(string defaultName)
    {
        Text = "Make PDF";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(400, 150);

        var nameLabel = new Label { Text = "Save as:", Left = 16, Top = 16, Width = 368 };
        _nameTextBox = new TextBox { Left = 16, Top = 40, Width = 320, Text = defaultName, MaxLength = 100 };
        var extensionLabel = new Label { Text = ".pdf", Left = 340, Top = 43, Width = 44 };

        _errorLabel = new Label
        {
            Left = 16,
            Top = 68,
            Width = 368,
            Height = 32,
            ForeColor = Color.Firebrick,
        };

        var okButton = new Button { Text = "Make PDF", Left = 208, Top = 108, Width = 84 };
        var cancelButton = new Button { Text = "Cancel", Left = 300, Top = 108, Width = 84, DialogResult = DialogResult.Cancel };

        okButton.Click += OkButton_Click;

        Controls.AddRange(new Control[]
        {
            nameLabel, _nameTextBox, extensionLabel, _errorLabel, okButton, cancelButton,
        });

        AcceptButton = okButton;
        CancelButton = cancelButton;
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        var nameResult = FileNameValidator.Validate($"{_nameTextBox.Text.Trim()}.pdf", maxLength: 104);
        if (!nameResult.IsValid)
        {
            _errorLabel.Text = nameResult.ErrorMessage;
            return;
        }

        OutputName = _nameTextBox.Text.Trim();
        DialogResult = DialogResult.OK;
        Close();
    }
}
