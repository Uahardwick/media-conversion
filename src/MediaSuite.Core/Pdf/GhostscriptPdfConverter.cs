using MediaSuite.Core.Processes;

namespace MediaSuite.Core.Pdf;

/// <summary>
/// Reads page counts and rasterizes individual PDF pages using Ghostscript
/// directly, deliberately bypassing ImageMagick's PDF coder (and its
/// security-policy gate) entirely.
/// </summary>
public sealed class GhostscriptPdfConverter
{
    private readonly string _ghostscriptExecutablePath;

    public GhostscriptPdfConverter(string ghostscriptExecutablePath)
    {
        _ghostscriptExecutablePath = ghostscriptExecutablePath;
    }

    public async Task<int> GetPageCountAsync(string pdfPath, CancellationToken cancellationToken = default)
    {
        var outputLines = new List<string>();
        var errorLines = new List<string>();
        var script = $"({EscapePostScriptString(pdfPath)}) (r) file runpdfbegin pdfpagecount = quit";

        await ProcessRunner.RunAsync(
            _ghostscriptExecutablePath,
            new[] { "-q", "-dNODISPLAY", "-dNOSAFER", "-c", script },
            onOutputLine: outputLines.Add,
            onErrorLine: errorLines.Add,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return ParsePageCount(pdfPath, outputLines, errorLines);
    }

    public async Task RenderPageAsync(
        string pdfPath,
        int pageNumber,
        string outputPngPath,
        int dpi = 300,
        CancellationToken cancellationToken = default)
    {
        var errorLines = new List<string>();

        await ProcessRunner.RunAsync(
            _ghostscriptExecutablePath,
            new[]
            {
                "-dBATCH",
                "-dNOPAUSE",
                "-dSAFER",
                "-sDEVICE=png16m",
                $"-r{dpi}",
                $"-dFirstPage={pageNumber}",
                $"-dLastPage={pageNumber}",
                "-dTextAlphaBits=4",
                "-dGraphicsAlphaBits=4",
                $"-sOutputFile={outputPngPath}",
                pdfPath,
            },
            onErrorLine: errorLines.Add,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!File.Exists(outputPngPath))
            throw new PdfReadException(pdfPath, string.Join(Environment.NewLine, errorLines));
    }

    internal static int ParsePageCount(string pdfPath, IReadOnlyList<string> outputLines, IReadOnlyList<string> errorLines)
    {
        var combinedError = string.Join(Environment.NewLine, errorLines);
        if (combinedError.Contains("password", StringComparison.OrdinalIgnoreCase))
            throw new PasswordProtectedPdfException(pdfPath);

        var lastLine = outputLines.LastOrDefault(line => !string.IsNullOrWhiteSpace(line));
        if (lastLine is null || !int.TryParse(lastLine.Trim(), out var pageCount) || pageCount <= 0)
            throw new PdfReadException(pdfPath, combinedError);

        return pageCount;
    }

    internal static string EscapePostScriptString(string value) =>
        value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
