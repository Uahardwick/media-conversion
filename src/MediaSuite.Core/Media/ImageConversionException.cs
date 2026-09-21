namespace MediaSuite.Core.Media;

public sealed class ImageConversionException : Exception
{
    public ImageConversionException(string imagePath, string details)
        : base(BuildMessage(imagePath, details))
    {
    }

    private static string BuildMessage(string imagePath, string details) =>
        string.IsNullOrWhiteSpace(details)
            ? $"\"{Path.GetFileName(imagePath)}\" couldn't be converted."
            : $"\"{Path.GetFileName(imagePath)}\" couldn't be converted.{Environment.NewLine}{details}";
}
