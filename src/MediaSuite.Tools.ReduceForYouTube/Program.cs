using MediaSuite.Core.FileSystem;
using MediaSuite.Core.Media;

namespace MediaSuite.Tools.ReduceForYouTube;

/// <summary>
/// Invoked by the "Reduce for YouTube PNG (under 2 MB)" Explorer right-click
/// verb with the clicked image's path as args[0].
/// </summary>
internal static class Program
{
    private const string Caption = "Reduce for YouTube";

    private static readonly string[] SupportedExtensions = { ".jpg", ".jpeg", ".png" };

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length == 0 || !File.Exists(args[0]))
        {
            ShowWarning("This tool must be run from the \"Reduce for YouTube\" right-click command on a JPG or PNG file.");
            return;
        }

        var inputPath = args[0];
        var extension = Path.GetExtension(inputPath);
        if (!SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            ShowWarning("This tool only supports JPG, JPEG, and PNG files.");
            return;
        }

        var imageMagickPath = ImageMagickLocator.Find();
        if (imageMagickPath is null)
        {
            ShowError("ImageMagick wasn't found on this computer. Install it from the Media Tools Suite Manager and try again.");
            return;
        }

        var directory = Path.GetDirectoryName(inputPath);
        if (directory is null)
        {
            ShowError("Couldn't determine the image's folder.");
            return;
        }

        var baseName = Path.GetFileNameWithoutExtension(inputPath);
        var outputPath = UniquePathGenerator.FindAvailable(
            attempt => attempt == 0
                ? Path.Combine(directory, $"{baseName}-YouTube.png")
                : Path.Combine(directory, $"{baseName} ({attempt})-YouTube.png"),
            File.Exists);

        var converter = new ImageMagickConverter(imageMagickPath);
        var service = new ReductionService(converter);

        using var progressForm = new ProgressForm();

        progressForm.Shown += async (_, _) =>
        {
            try
            {
                await service.ReduceAsync(inputPath, outputPath, CancellationToken.None).ConfigureAwait(true);
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
