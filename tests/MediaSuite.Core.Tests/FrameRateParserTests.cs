using MediaSuite.Core.Media;
using Xunit;

namespace MediaSuite.Core.Tests;

public class FrameRateParserTests
{
    [Theory]
    [InlineData("30", 30.0)]
    [InlineData("29.97", 29.97)]
    [InlineData("23.976", 23.976)]
    public void DecimalFrameRate_Parses(string input, double expected)
    {
        var result = FrameRateParser.Parse(input);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.FramesPerSecond, precision: 5);
        Assert.Equal(input, result.FfmpegArgument);
    }

    [Fact]
    public void FractionalFrameRate_Parses()
    {
        var result = FrameRateParser.Parse("30000/1001");

        Assert.True(result.IsValid);
        Assert.Equal(30000.0 / 1001.0, result.FramesPerSecond, precision: 5);
        Assert.Equal("30000/1001", result.FfmpegArgument);
    }

    [Fact]
    public void TrimsWhitespace()
    {
        var result = FrameRateParser.Parse("  30  ");

        Assert.True(result.IsValid);
        Assert.Equal(30.0, result.FramesPerSecond);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("30/0")]
    [InlineData("-30/1001")]
    [InlineData("30/-1001")]
    public void InvalidFrameRates_Fail(string input)
    {
        Assert.False(FrameRateParser.Parse(input).IsValid);
    }
}
