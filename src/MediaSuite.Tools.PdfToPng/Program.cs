namespace MediaSuite.Tools.PdfToPng;

/// <summary>
/// Invoked by the "Convert PDF to PNG" Explorer right-click verb with the
/// clicked PDF's path as args[0]. Not yet implemented beyond the stub below.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        MessageBox.Show(
            "PDF to PNG hasn't been implemented yet.",
            "Media Tools Suite",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
