using MediaSuite.Core.FileSystem;
using MediaSuite.Core.Media;

namespace MediaSuite.Tools.FfmpegConvert;

internal sealed class ConversionService
{
    private readonly FfmpegVideoConverter _converter;

    public ConversionService(FfmpegVideoConverter converter)
    {
        _converter = converter;
    }

    public async Task ConvertAsync(
        string inputPath,
        string? frameRateArgument,
        IProgress<double> progress,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(inputPath)
            ?? throw new InvalidOperationException("Couldn't determine the source file's folder.");
        var baseName = Path.GetFileNameWithoutExtension(inputPath);

        var finalOutputPath = UniquePathGenerator.FindAvailable(
            attempt => attempt == 0
                ? Path.Combine(directory, $"{baseName}-converted.mp4")
                : Path.Combine(directory, $"{baseName}-converted ({attempt + 1}).mp4"),
            File.Exists);

        var tempOutputPath = finalOutputPath + ".tmp";

        try
        {
            await _converter.ConvertToMp4Async(inputPath, tempOutputPath, frameRateArgument, progress, cancellationToken)
                .ConfigureAwait(false);

            File.Move(tempOutputPath, finalOutputPath);
        }
        finally
        {
            if (File.Exists(tempOutputPath))
                File.Delete(tempOutputPath);
        }
    }
}
