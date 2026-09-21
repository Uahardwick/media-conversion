namespace MediaSuite.Tools.MakePdf;

internal sealed class WorkingForm : Form
{
    public WorkingForm(string message)
    {
        Text = "Make PDF";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(320, 80);

        var label = new Label { Left = 16, Top = 16, Width = 288, Text = message };
        var progressBar = new ProgressBar { Left = 16, Top = 44, Width = 288, Style = ProgressBarStyle.Marquee };

        Controls.AddRange(new Control[] { label, progressBar });
    }
}
