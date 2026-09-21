using MediaSuite.Core.Media;
using Xunit;

namespace MediaSuite.Core.Tests;

public class ImageMagickLocatorTests
{
    [Theory]
    [InlineData("ImageMagick-7.1.1-Q16-HDRI", "7.1.1")]
    [InlineData("ImageMagick-7.0.11-14-Q16", "7.0.11")]
    public void ParseVersion_ExtractsLeadingVersionNumber(string folderName, string expectedVersion)
    {
        Assert.Equal(Version.Parse(expectedVersion), ImageMagickLocator.ParseVersion(folderName));
    }

    [Fact]
    public void ParseVersion_ReturnsZero_WhenNoVersionFound()
    {
        Assert.Equal(new Version(0, 0), ImageMagickLocator.ParseVersion("ImageMagick"));
    }
}
