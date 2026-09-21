using MediaSuite.Core.Media;
using Xunit;

namespace MediaSuite.Core.Tests;

public class FfmpegVideoConverterTests
{
    [Theory]
    [InlineData("  Duration: 00:01:23.45, start: 0.000000, bitrate: 798 kb/s", 83.45)]
    [InlineData("Duration: 01:02:03.00, start: 0.000000, bitrate: N/A", 3723.0)]
    public void TryParseDurationLine_ExtractsTotalSeconds(string line, double expectedSeconds)
    {
        var duration = FfmpegVideoConverter.TryParseDurationLine(line);

        Assert.NotNull(duration);
        Assert.Equal(expectedSeconds, duration!.Value.TotalSeconds, precision: 2);
    }

    [Fact]
    public void TryParseDurationLine_ReturnsNull_WhenLineHasNoDuration()
    {
        Assert.Null(FfmpegVideoConverter.TryParseDurationLine("frame=   91 fps=0.0"));
    }

    [Theory]
    [InlineData("out_time=00:00:03.018594", 3.018594)]
    [InlineData("out_time=00:01:00.000000", 60.0)]
    public void TryParseOutTimeLine_ExtractsElapsedSeconds(string line, double expectedSeconds)
    {
        var elapsed = FfmpegVideoConverter.TryParseOutTimeLine(line);

        Assert.NotNull(elapsed);
        Assert.Equal(expectedSeconds, elapsed!.Value.TotalSeconds, precision: 3);
    }

    [Fact]
    public void TryParseOutTimeLine_ReturnsNull_WhenLineIsNotOutTime()
    {
        Assert.Null(FfmpegVideoConverter.TryParseOutTimeLine("out_time_ms=3018594"));
    }
}
