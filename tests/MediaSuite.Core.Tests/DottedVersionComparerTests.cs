using MediaSuite.Core.Updates;
using Xunit;

namespace MediaSuite.Core.Tests;

public class DottedVersionComparerTests
{
    [Theory]
    [InlineData("10.08.0", "10.2.1")]
    [InlineData("7.1.2.31", "7.1.2.30")]
    [InlineData("9.0.2", "6.1.1")]
    [InlineData("1.0", "0.9.9")]
    public void IsNewer_ReturnsTrue_WhenLatestIsNumericallyGreater(string latest, string installed)
    {
        Assert.True(DottedVersionComparer.IsNewer(latest, installed));
    }

    [Theory]
    [InlineData("10.2.1", "10.08.0")]
    [InlineData("7.1.2.30", "7.1.2.31")]
    [InlineData("6.1.1", "9.0.2")]
    public void IsNewer_ReturnsFalse_WhenLatestIsNotGreater(string latest, string installed)
    {
        Assert.False(DottedVersionComparer.IsNewer(latest, installed));
    }

    [Fact]
    public void IsNewer_ReturnsFalse_WhenVersionsAreEqual()
    {
        Assert.False(DottedVersionComparer.IsNewer("7.1.2.31", "7.1.2.31"));
    }

    [Fact]
    public void IsNewer_HandlesDifferentSegmentCounts()
    {
        Assert.True(DottedVersionComparer.IsNewer("7.1.3", "7.1"));
        Assert.False(DottedVersionComparer.IsNewer("7.1", "7.1.0"));
    }
}
