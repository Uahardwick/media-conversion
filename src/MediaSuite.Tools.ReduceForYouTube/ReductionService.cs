using MediaSuite.Core.Media;

namespace MediaSuite.Tools.ReduceForYouTube;

internal sealed class ReductionService
{
    private const long MaxBytes = 2_000_000;

    // Practical floor: virtually any real image compresses well under 2MB
    // by the time it's this narrow, so searching narrower buys nothing.
    private const int MinimumWidth = 16;

    private readonly ImageMagickConverter _converter;

    public ReductionService(ImageMagickConverter converter)
    {
        _converter = converter;
    }

    public async Task ReduceAsync(string inputPath, string outputPath, CancellationToken cancellationToken)
    {
        var searchScratchPath = outputPath + ".search.tmp";
        var finalScratchPath = outputPath + ".tmp";

        try
        {
            await _converter.ConvertToPngAsync(inputPath, finalScratchPath, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (new FileInfo(finalScratchPath).Length < MaxBytes)
            {
                File.Move(finalScratchPath, outputPath);
                return;
            }

            var originalWidth = await _converter.GetWidthAsync(inputPath, cancellationToken).ConfigureAwait(false);

            var low = MinimumWidth;
            var high = Math.Max(originalWidth - 1, MinimumWidth);
            var bestWidth = MinimumWidth;

            while (low <= high)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var mid = low + ((high - low) / 2);
                await _converter.ConvertToPngAsync(inputPath, searchScratchPath, mid, cancellationToken).ConfigureAwait(false);

                if (new FileInfo(searchScratchPath).Length < MaxBytes)
                {
                    bestWidth = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            await _converter.ConvertToPngAsync(inputPath, finalScratchPath, bestWidth, cancellationToken).ConfigureAwait(false);
            File.Move(finalScratchPath, outputPath);
        }
        finally
        {
            if (File.Exists(searchScratchPath))
                File.Delete(searchScratchPath);

            if (File.Exists(finalScratchPath))
                File.Delete(finalScratchPath);
        }
    }
}
