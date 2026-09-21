using MediaSuite.Core.Pdf;

namespace MediaSuite.Tools.MergePdf;

/// <summary>
/// Invoked by the "Merge PDFs" Explorer right-click verb with the clicked
/// PDF's path as args[0].
/// </summary>
internal static class Program
{
    private const string Caption = "Merge PDFs";

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length == 0 || !File.Exists(args[0]))
        {
            ShowWarning("This tool must be run from the \"Merge PDFs\" right-click command on a PDF file.");
            return;
        }

        var clickedPdfPath = args[0];

        var ghostscriptPath = GhostscriptLocator.Find();
        if (ghostscriptPath is null)
        {
            ShowError("Ghostscript wasn't found on this computer. Install it from the Media Tools Suite Manager and try again.");
            return;
        }

        var converter = new GhostscriptPdfConverter(ghostscriptPath);

        if (!TryCheckReadable(converter, clickedPdfPath))
            return;

        using var optionsForm = new MergeOptionsForm(clickedPdfPath);
        if (optionsForm.ShowDialog() != DialogResult.OK)
            return;

        if (!TryCheckReadable(converter, optionsForm.OtherPdfPath))
            return;

        var directory = Path.GetDirectoryName(clickedPdfPath);
        if (directory is null)
        {
            ShowError("Couldn't determine the PDF's folder.");
            return;
        }

        var outputPath = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(clickedPdfPath)}-merged.pdf");
        if (File.Exists(outputPath))
        {
            var choice = MessageBox.Show(
                $"\"{Path.GetFileName(outputPath)}\" already exists. Overwrite it?",
                Caption,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (choice != DialogResult.Yes)
                return;
        }

        var order = optionsForm.Position == MergePosition.Front
            ? new[] { optionsForm.OtherPdfPath, clickedPdfPath }
            : new[] { clickedPdfPath, optionsForm.OtherPdfPath };

        using var workingForm = new WorkingForm("Merging PDFs...");

        workingForm.Shown += async (_, _) =>
        {
            try
            {
                await converter.MergeAsync(order, outputPath).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                workingForm.Close();
            }
        };

        workingForm.ShowDialog();
    }

    private static bool TryCheckReadable(GhostscriptPdfConverter converter, string pdfPath)
    {
        try
        {
            converter.GetPageCountAsync(pdfPath).GetAwaiter().GetResult();
            return true;
        }
        catch (PasswordProtectedPdfException ex)
        {
            ShowWarning(ex.Message);
            return false;
        }
        catch (PdfReadException ex)
        {
            ShowError(ex.Message);
            return false;
        }
    }

    private static void ShowWarning(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static void ShowError(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
}
