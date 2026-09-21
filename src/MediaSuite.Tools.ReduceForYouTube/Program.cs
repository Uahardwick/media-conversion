namespace MediaSuite.Tools.ReduceForYouTube;

/// <summary>
/// Invoked by the "Reduce for YouTube PNG (under 2 MB)" Explorer right-click
/// verb with the clicked image's path as args[0]. Not yet implemented beyond
/// the stub below.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        MessageBox.Show(
            "Reduce for YouTube hasn't been implemented yet.",
            "Media Tools Suite",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
