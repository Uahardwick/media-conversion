namespace MediaSuite.Tools.MakePdf;

/// <summary>
/// Invoked by the "Make PDF" Explorer right-click verb with the clicked
/// Word or PowerPoint file's path as args[0].
/// </summary>
internal static class Program
{
    private const string Caption = "Make PDF";

    private static readonly string[] WordExtensions = { ".doc", ".docx" };
    private static readonly string[] PowerPointExtensions = { ".ppt", ".pptx" };

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length == 0 || !File.Exists(args[0]))
        {
            ShowWarning("This tool must be run from the \"Make PDF\" right-click command on a Word or PowerPoint file.");
            return;
        }

        var inputPath = args[0];
        var extension = Path.GetExtension(inputPath);

        var isWord = WordExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        var isPowerPoint = PowerPointExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        if (!isWord && !isPowerPoint)
        {
            ShowWarning("This tool only supports Word (.doc, .docx) and PowerPoint (.ppt, .pptx) files.");
            return;
        }

        var directory = Path.GetDirectoryName(inputPath);
        if (directory is null)
        {
            ShowError("Couldn't determine the file's folder.");
            return;
        }

        var defaultName = Path.GetFileNameWithoutExtension(inputPath);
        if (defaultName.Length > 100)
            defaultName = defaultName[..100];

        using var optionsForm = new OptionsForm(defaultName);
        if (optionsForm.ShowDialog() != DialogResult.OK)
            return;

        var outputPath = Path.Combine(directory, $"{optionsForm.OutputName}.pdf");
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

        using var workingForm = new WorkingForm("Converting to PDF...");
        Exception? failure = null;

        workingForm.Shown += (_, _) =>
        {
            try
            {
                if (isWord)
                    OfficeConverter.ConvertWordToPdf(inputPath, outputPath);
                else
                    OfficeConverter.ConvertPowerPointToPdf(inputPath, outputPath);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                workingForm.Close();
            }
        };

        workingForm.ShowDialog();

        if (failure is not null)
            ShowError(failure.Message);
    }

    private static void ShowWarning(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static void ShowError(string message) =>
        MessageBox.Show(message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
}
