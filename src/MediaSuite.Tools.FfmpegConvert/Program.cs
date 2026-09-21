using MediaSuite.Core.Media;

namespace MediaSuite.Tools.FfmpegConvert;

/// <summary>
/// Invoked by the "Convert with FFMPEG" Explorer right-click verb with the
/// clicked video's path as args[0].
/// </summary>
internal static class Program
{
    private const string Caption = "Convert with FFMPEG";

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length == 0 || !File.Exists(args[0]))
        {
            ShowWarning("This tool must be run from the \"Convert with FFMPEG\" right-click command on a video file.");
            return;
        }

        var inputPath = args[0];

        var ffmpegPath = FfmpegLocator.Find();
        if (ffmpegPath is null)
        {
            ShowError("FFmpeg wasn't found on this computer. Install it from the Media Tools Suite Manager and try again.");
            return;
        }

        using var optionsForm = new OptionsForm();
        if (optionsForm.ShowDialog() != DialogResult.OK)
            return;

        var converter = new FfmpegVideoConverter(ffmpegPath);
        var service = new ConversionService(converter);

        using var progressForm = new ProgressForm();
        var progress = new Progress<double>(progressForm.ReportProgress);

        progressForm.Shown += async (_, _) =>
        {
            try
            {
                await service.ConvertAsync(inputPath, optionsForm.FrameRateArgument, progress, progressForm.CancellationToken)
                    .ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                // User cancelled; ConversionService already removed the temporary output.
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                progressForm.Close();
            }
        };

        progressForm.ShowDialog();
    }

    private static void ShowWarning(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static void ShowError(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
}
