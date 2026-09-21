namespace MediaSuite.Tools.OfficeToPdf;

/// <summary>
/// Invoked by the "Convert to PDF" Explorer right-click verb with the clicked
/// Word or PowerPoint file's path as args[0]. Not yet implemented beyond the
/// stub below.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        MessageBox.Show(
            "Office to PDF hasn't been implemented yet.",
            "Media Tools Suite",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
