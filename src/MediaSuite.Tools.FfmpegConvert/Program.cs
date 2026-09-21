namespace MediaSuite.Tools.FfmpegConvert;

/// <summary>
/// Invoked by the "Convert with FFMPEG" Explorer right-click verb with the
/// clicked video's path as args[0]. Not yet implemented beyond the stub below.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        MessageBox.Show(
            "Convert with FFMPEG hasn't been implemented yet.",
            "Media Tools Suite",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
