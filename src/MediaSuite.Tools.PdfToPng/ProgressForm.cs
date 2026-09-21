namespace MediaSuite.Tools.PdfToPng;

internal sealed class ProgressForm : Form
{
    private readonly ProgressBar _progressBar;
    private readonly Label _statusLabel;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public ProgressForm(int totalPages)
    {
        Text = "Converting PDF to PNG";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(360, 110);

        _statusLabel = new Label { Left = 16, Top = 16, Width = 328, Text = $"Rendering page 0 of {totalPages}..." };
        _progressBar = new ProgressBar { Left = 16, Top = 44, Width = 328, Minimum = 0, Maximum = Math.Max(totalPages, 1) };

        var cancelButton = new Button { Text = "Cancel", Left = 268, Top = 74, Width = 76 };
        cancelButton.Click += (_, _) => _cancellationTokenSource.Cancel();

        Controls.AddRange(new Control[] { _statusLabel, _progressBar, cancelButton });
    }

    public void ReportProgress(int completed, int total)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => ReportProgress(completed, total));
            return;
        }

        _progressBar.Value = Math.Min(completed, _progressBar.Maximum);
        _statusLabel.Text = $"Rendering page {completed} of {total}...";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _cancellationTokenSource.Dispose();

        base.Dispose(disposing);
    }
}
