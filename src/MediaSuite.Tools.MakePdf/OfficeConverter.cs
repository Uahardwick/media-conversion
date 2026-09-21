using System.Runtime.InteropServices;

namespace MediaSuite.Tools.MakePdf;

/// <summary>
/// Converts Word and PowerPoint files to PDF via COM automation of the
/// team's own installed Office apps. Uses late-bound dynamic dispatch
/// against the registered ProgID rather than the Microsoft.Office.Interop
/// primary interop assemblies, so this doesn't pin the suite to a specific
/// Office version or need Office present at build time - only at run time.
/// </summary>
internal static class OfficeConverter
{
    // WdExportFormat.wdExportFormatPDF
    private const int WdExportFormatPdf = 17;

    // PpSaveAsFileType.ppSaveAsPDF
    private const int PpSaveAsPdf = 32;

    public static void ConvertWordToPdf(string inputPath, string outputPath)
    {
        var wordType = Type.GetTypeFromProgID("Word.Application")
            ?? throw new OfficeNotInstalledException("Microsoft Word");

        dynamic? word = null;
        dynamic? document = null;
        try
        {
            word = Activator.CreateInstance(wordType)!;
            word.Visible = false;
            word.DisplayAlerts = 0; // wdAlertsNone

            document = word.Documents.Open(FileName: inputPath, ReadOnly: true, AddToRecentFiles: false);
            document.ExportAsFixedFormat(OutputFileName: outputPath, ExportFormat: WdExportFormatPdf);
        }
        catch (Exception ex) when (ex is not OfficeNotInstalledException)
        {
            throw new OfficeConversionException(inputPath, ex);
        }
        finally
        {
            SafeClose(document);
            SafeQuit(word);
            ReleaseComObject(document);
            ReleaseComObject(word);
        }
    }

    public static void ConvertPowerPointToPdf(string inputPath, string outputPath)
    {
        var powerPointType = Type.GetTypeFromProgID("PowerPoint.Application")
            ?? throw new OfficeNotInstalledException("Microsoft PowerPoint");

        dynamic? powerPoint = null;
        dynamic? presentation = null;
        try
        {
            powerPoint = Activator.CreateInstance(powerPointType)!;

            // PowerPoint's automation model doesn't support Application.Visible
            // = false the way Word does; WithWindow: false is the standard way
            // to keep the opened presentation from showing a window.
            presentation = powerPoint.Presentations.Open(
                FileName: inputPath,
                ReadOnly: true,
                Untitled: false,
                WithWindow: false);

            presentation.SaveAs(FileName: outputPath, FileFormat: PpSaveAsPdf);
        }
        catch (Exception ex) when (ex is not OfficeNotInstalledException)
        {
            throw new OfficeConversionException(inputPath, ex);
        }
        finally
        {
            SafeClose(presentation);
            SafeQuit(powerPoint);
            ReleaseComObject(presentation);
            ReleaseComObject(powerPoint);
        }
    }

    private static void SafeClose(dynamic? documentOrPresentation)
    {
        try
        {
            documentOrPresentation?.Close();
        }
        catch (COMException)
        {
        }
    }

    private static void SafeQuit(dynamic? application)
    {
        try
        {
            application?.Quit();
        }
        catch (COMException)
        {
        }
    }

    private static void ReleaseComObject(object? comObject)
    {
        if (comObject is not null && Marshal.IsComObject(comObject))
            Marshal.ReleaseComObject(comObject);
    }
}
