using MediaSuite.Core.Processes;

namespace MediaSuite.Core.Media;

/// <summary>
/// Thin wrapper over ImageMagick's "magick" executable for reading an
/// image's pixel width and converting it to PNG, optionally capped to a
/// maximum width (never upscaled). Always reads only the first frame,
/// applies EXIF orientation, strips metadata, forces 8-bit sRGB, and
/// forces truecolor+alpha (PNG color type 6) so output is never
/// palette-reduced and transparency is always preserved.
/// </summary>
public sealed class ImageMagickConverter
{
    private readonly string _imageMagickExecutablePath;

    public ImageMagickConverter(string imageMagickExecutablePath)
    {
        _imageMagickExecutablePath = imageMagickExecutablePath;
    }

    public async Task<int> GetWidthAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        var outputLines = new List<string>();
        var errorLines = new List<string>();

        var result = await ProcessRunner.RunAsync(
            _imageMagickExecutablePath,
            new[] { "identify", "-format", "%w", $"{imagePath}[0]" },
            onOutputLine: outputLines.Add,
            onErrorLine: errorLines.Add,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var text = string.Concat(outputLines).Trim();
        if (result.ExitCode != 0 || !int.TryParse(text, out var width) || width <= 0)
            throw new ImageConversionException(imagePath, string.Join(Environment.NewLine, errorLines.TakeLast(20)));

        return width;
    }

    public async Task ConvertToPngAsync(
        string inputPath,
        string outputPath,
        int? maxWidth = null,
        CancellationToken cancellationToken = default)
    {
        var arguments = new List<string>
        {
            "convert",
            $"{inputPath}[0]",
            "-auto-orient",
            "-strip",
            "-colorspace", "sRGB",
            "-depth", "8",
            "-define", "png:color-type=6",
        };

        if (maxWidth is { } width)
        {
            arguments.Add("-resize");
            arguments.Add($"{width}x>");
        }

        arguments.Add("-quality");
        arguments.Add("95");
        arguments.Add(outputPath);

        var errorLines = new List<string>();
        var result = await ProcessRunner.RunAsync(
            _imageMagickExecutablePath,
            arguments,
            onErrorLine: errorLines.Add,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.ExitCode != 0 || !File.Exists(outputPath))
            throw new ImageConversionException(inputPath, string.Join(Environment.NewLine, errorLines.TakeLast(20)));
    }
}
