using MediaSuite.Core.Pdf;
using Xunit;

namespace MediaSuite.Core.Tests;

public class GhostscriptPdfConverterTests
{
    [Fact]
    public void ParsePageCount_ReturnsCount_WhenOutputIsAPositiveNumber()
    {
        var count = GhostscriptPdfConverter.ParsePageCount("doc.pdf", new[] { "3" }, Array.Empty<string>());

        Assert.Equal(3, count);
    }

    [Fact]
    public void ParsePageCount_Throws_WhenErrorMentionsPassword()
    {
        Assert.Throws<PasswordProtectedPdfException>(() =>
            GhostscriptPdfConverter.ParsePageCount(
                "doc.pdf",
                new[] { "0" },
                new[] { "**** This file requires a password for access." }));
    }

    [Fact]
    public void ParsePageCount_Throws_WhenOutputIsNotANumber()
    {
        Assert.Throws<PdfReadException>(() =>
            GhostscriptPdfConverter.ParsePageCount("doc.pdf", Array.Empty<string>(), new[] { "some error" }));
    }

    [Fact]
    public void ParsePageCount_Throws_WhenCountIsZero()
    {
        Assert.Throws<PdfReadException>(() =>
            GhostscriptPdfConverter.ParsePageCount("doc.pdf", new[] { "0" }, Array.Empty<string>()));
    }

    [Theory]
    [InlineData("hello (world)", "hello \\(world\\)")]
    [InlineData(@"C:\Temp\a.pdf", @"C:\\Temp\\a.pdf")]
    public void EscapePostScriptString_EscapesParensAndBackslashes(string input, string expected)
    {
        Assert.Equal(expected, GhostscriptPdfConverter.EscapePostScriptString(input));
    }

    [Theory]
    [InlineData("gs10.02.1", "10.2.1")]
    [InlineData("gs9.56.1", "9.56.1")]
    public void ParseVersion_ParsesTrailingDigits(string folderName, string expectedVersion)
    {
        Assert.Equal(Version.Parse(expectedVersion), GhostscriptLocator.ParseVersion(folderName));
    }
}
