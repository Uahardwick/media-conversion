namespace MediaSuite.Tools.FfmpegConvert;

internal sealed class ProgressForm : Form
{
    private readonly ProgressBar _progressBar;
    private readonly Label _statusLabel;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public ProgressForm()
    {
        Text = "Converting with FFmpeg";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(360, 110);

        _statusLabel = new Label { Left = 16, Top = 16, Width = 328, Text = "Converting..." };
        _progressBar = new ProgressBar { Left = 16, Top = 44, Width = 328, Minimum = 0, Maximum = 100 };

        var cancelButton = new Button { Text = "Cancel", Left = 268, Top = 74, Width = 76 };
        cancelButton.Click += (_, _) => _cancellationTokenSource.Cancel();

        Controls.AddRange(new Control[] { _statusLabel, _progressBar, cancelButton });
    }

    public void ReportProgress(double fraction)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => ReportProgress(fraction));
            return;
        }

        var percent = (int)Math.Round(Math.Clamp(fraction, 0, 1) * 100);
        _progressBar.Value = percent;
        _statusLabel.Text = $"Converting... {percent}%";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _cancellationTokenSource.Dispose();

        base.Dispose(disposing);
    }
}
