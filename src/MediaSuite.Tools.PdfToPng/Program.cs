using MediaSuite.Core.Pdf;

namespace MediaSuite.Tools.PdfToPng;

/// <summary>
/// Invoked by the "Convert PDF to PNG" Explorer right-click verb with the
/// clicked PDF's path as args[0].
/// </summary>
internal static class Program
{
    private const string Caption = "Convert PDF to PNG";

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length == 0 || !File.Exists(args[0]))
        {
            ShowWarning("This tool must be run from the \"Convert PDF to PNG\" right-click command on a PDF file.");
            return;
        }

        var pdfPath = args[0];

        var ghostscriptPath = GhostscriptLocator.Find();
        if (ghostscriptPath is null)
        {
            ShowError("Ghostscript wasn't found on this computer. Install it from the Media Tools Suite Manager and try again.");
            return;
        }

        var converter = new GhostscriptPdfConverter(ghostscriptPath);

        int totalPages;
        try
        {
            totalPages = converter.GetPageCountAsync(pdfPath).GetAwaiter().GetResult();
        }
        catch (PasswordProtectedPdfException ex)
        {
            ShowWarning(ex.Message);
            return;
        }
        catch (PdfReadException ex)
        {
            ShowError(ex.Message);
            return;
        }

        var defaultName = Path.GetFileNameWithoutExtension(pdfPath);
        if (defaultName.Length > 100)
            defaultName = defaultName[..100];

        using var optionsForm = new OptionsForm(defaultName, totalPages);
        if (optionsForm.ShowDialog() != DialogResult.OK)
            return;

        var pdfDirectory = Path.GetDirectoryName(pdfPath);
        if (pdfDirectory is null)
        {
            ShowError("Couldn't determine the PDF's folder.");
            return;
        }

        try
        {
            ConversionService.EnsureWriteAccess(pdfDirectory);
            ConversionService.EnsureSufficientDiskSpace(pdfDirectory, optionsForm.Pages.Count);
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message);
            return;
        }

        var service = new ConversionService(converter);
        using var progressForm = new ProgressForm(optionsForm.Pages.Count);
        var progress = new Progress<(int Completed, int Total)>(p => progressForm.ReportProgress(p.Completed, p.Total));

        progressForm.Shown += async (_, _) =>
        {
            try
            {
                await service.ConvertAsync(
                    optionsForm.Pages,
                    pdfPath,
                    optionsForm.OutputName,
                    progress,
                    progressForm.CancellationToken).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                // User cancelled; ConversionService already removed the temporary folder.
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
