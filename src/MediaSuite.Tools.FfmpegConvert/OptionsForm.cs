using MediaSuite.Core.Media;

namespace MediaSuite.Tools.FfmpegConvert;

internal sealed class OptionsForm : Form
{
    private readonly TextBox _frameRateTextBox;
    private readonly CheckBox _keepCurrentCheckBox;
    private readonly Label _errorLabel;

    public string? FrameRateArgument { get; private set; }

    public OptionsForm()
    {
        Text = "Convert with FFMPEG";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(360, 190);

        var frameRateLabel = new Label { Text = "Frame rate:", Left = 16, Top = 16, Width = 328 };
        _frameRateTextBox = new TextBox { Left = 16, Top = 40, Width = 328, Text = "30" };

        var hintLabel = new Label
        {
            Text = "Examples: 30  |  29.97  |  30000/1001",
            Left = 16,
            Top = 68,
            Width = 328,
            ForeColor = SystemColors.GrayText,
        };

        _keepCurrentCheckBox = new CheckBox { Text = "Keep current frame rate", Left = 16, Top = 96, Width = 328 };
        _keepCurrentCheckBox.CheckedChanged += (_, _) => _frameRateTextBox.Enabled = !_keepCurrentCheckBox.Checked;

        _errorLabel = new Label
        {
            Left = 16,
            Top = 122,
            Width = 328,
            Height = 32,
            ForeColor = Color.Firebrick,
        };

        var okButton = new Button { Text = "Convert", Left = 168, Top = 158, Width = 84 };
        var cancelButton = new Button { Text = "Cancel", Left = 260, Top = 158, Width = 84, DialogResult = DialogResult.Cancel };

        okButton.Click += OkButton_Click;

        Controls.AddRange(new Control[]
        {
            frameRateLabel, _frameRateTextBox, hintLabel, _keepCurrentCheckBox, _errorLabel, okButton, cancelButton,
        });

        AcceptButton = okButton;
        CancelButton = cancelButton;
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        if (_keepCurrentCheckBox.Checked)
        {
            FrameRateArgument = null;
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        var result = FrameRateParser.Parse(_frameRateTextBox.Text);
        if (!result.IsValid)
        {
            _errorLabel.Text = result.ErrorMessage;
            return;
        }

        FrameRateArgument = result.FfmpegArgument;
        DialogResult = DialogResult.OK;
        Close();
    }
}
