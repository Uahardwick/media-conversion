namespace MediaSuite.Core.Pdf;

public sealed class PasswordProtectedPdfException : Exception
{
    public PasswordProtectedPdfException(string pdfPath)
        : base($"\"{Path.GetFileName(pdfPath)}\" is password-protected and can't be converted.")
    {
    }
}

public sealed class PdfReadException : Exception
{
    public PdfReadException(string pdfPath, string details)
        : base(BuildMessage(pdfPath, details))
    {
    }

    private static string BuildMessage(string pdfPath, string details) =>
        string.IsNullOrWhiteSpace(details)
            ? $"\"{Path.GetFileName(pdfPath)}\" couldn't be read as a PDF."
            : $"\"{Path.GetFileName(pdfPath)}\" couldn't be read as a PDF.{Environment.NewLine}{details}";
}
