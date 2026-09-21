using MediaSuite.Core.FileSystem;
using MediaSuite.Core.Pdf;

namespace MediaSuite.Tools.PdfToPng;

internal sealed class ConversionService
{
    private const int Dpi = 300;

    // Conservative worst-case estimate for a single 300-DPI, 24-bit PNG page
    // (a full-bleed photographic page can still land in single-digit MB after
    // compression; this leaves comfortable headroom without being absurd).
    private const long EstimatedBytesPerPage = 20L * 1024 * 1024;
    private const long MinimumFreeSpaceBuffer = 50L * 1024 * 1024;

    private readonly GhostscriptPdfConverter _converter;

    public ConversionService(GhostscriptPdfConverter converter)
    {
        _converter = converter;
    }

    public static void EnsureWriteAccess(string directory)
    {
        var probePath = Path.Combine(directory, $".mediasuite-write-test-{Guid.NewGuid():N}");
        try
        {
            using (File.Create(probePath))
            {
            }

            File.Delete(probePath);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            throw new InvalidOperationException("You don't have permission to save files next to this PDF.", ex);
        }
    }

    public static void EnsureSufficientDiskSpace(string destinationDirectory, int pageCount)
    {
        var root = Path.GetPathRoot(destinationDirectory);
        if (string.IsNullOrEmpty(root))
            return;

        DriveInfo drive;
        try
        {
            // DriveInfo only understands local drive letters; a UNC network
            // path (\\server\share\...) throws here, in which case we simply
            // can't verify free space and skip the check rather than fail.
            drive = new DriveInfo(root);
        }
        catch (ArgumentException)
        {
            return;
        }

        var requiredBytes = (pageCount * EstimatedBytesPerPage) + MinimumFreeSpaceBuffer;
        if (drive.AvailableFreeSpace < requiredBytes)
            throw new InvalidOperationException($"Not enough free disk space on {root} to complete this conversion.");
    }

    public async Task ConvertAsync(
        IReadOnlyList<int> pages,
        string pdfPath,
        string outputName,
        IProgress<(int Completed, int Total)> progress,
        CancellationToken cancellationToken)
    {
        var pdfDirectory = Path.GetDirectoryName(pdfPath)
            ?? throw new InvalidOperationException("Couldn't determine the PDF's folder.");

        var tempDirectory = Path.Combine(pdfDirectory, $".mediasuite-tmp-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        File.SetAttributes(tempDirectory, File.GetAttributes(tempDirectory) | FileAttributes.Hidden);

        try
        {
            for (var i = 0; i < pages.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var pageNumber = pages[i];
                var outputPath = Path.Combine(tempDirectory, $"{outputName} - Page {pageNumber}.png");
                await _converter.RenderPageAsync(pdfPath, pageNumber, outputPath, Dpi, cancellationToken)
                    .ConfigureAwait(false);

                progress.Report((i + 1, pages.Count));
            }

            var destinationFolder = UniquePathGenerator.FindAvailable(
                attempt => attempt == 0
                    ? Path.Combine(pdfDirectory, outputName)
                    : Path.Combine(pdfDirectory, $"{outputName} ({attempt + 1})"),
                Directory.Exists);

            Directory.Move(tempDirectory, destinationFolder);

            // Directory.Move preserves attributes, so the Hidden flag set
            // above would otherwise carry over onto the final, visible folder.
            File.SetAttributes(destinationFolder, File.GetAttributes(destinationFolder) & ~FileAttributes.Hidden);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, recursive: true);
        }
    }
}
