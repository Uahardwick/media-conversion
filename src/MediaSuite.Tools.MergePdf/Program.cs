namespace MediaSuite.Tools.MergePdf;

/// <summary>
/// Invoked by the "Merge PDFs" Explorer right-click verb with the clicked
/// PDF's path as args[0]. Not yet implemented beyond the stub below.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        MessageBox.Show(
            "Merge PDF hasn't been implemented yet.",
            "Media Tools Suite",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
