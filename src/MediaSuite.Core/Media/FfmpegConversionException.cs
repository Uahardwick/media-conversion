namespace MediaSuite.Core.Media;

public sealed class FfmpegConversionException : Exception
{
    public FfmpegConversionException(string inputPath, string details)
        : base(BuildMessage(inputPath, details))
    {
    }

    private static string BuildMessage(string inputPath, string details) =>
        string.IsNullOrWhiteSpace(details)
            ? $"\"{Path.GetFileName(inputPath)}\" couldn't be converted."
            : $"\"{Path.GetFileName(inputPath)}\" couldn't be converted.{Environment.NewLine}{details}";
}
