namespace MediaSuite.Tools.MakePdf;

public sealed class OfficeNotInstalledException : Exception
{
    public OfficeNotInstalledException(string applicationName)
        : base($"{applicationName} isn't installed on this computer, so this file can't be converted to PDF.")
    {
    }
}

public sealed class OfficeConversionException : Exception
{
    public OfficeConversionException(string inputPath, Exception innerException)
        : base($"\"{Path.GetFileName(inputPath)}\" couldn't be converted to PDF.{Environment.NewLine}{innerException.Message}", innerException)
    {
    }
}
