using MediaSuite.Core.Updates;
using Xunit;

namespace MediaSuite.Core.Tests;

public class InstalledVersionParsersTests
{
    [Theory]
    [InlineData("10.02.1", "10.02.1")]
    [InlineData("  10.08.0  \n", "10.08.0")]
    public void ParseGhostscriptVersion_TrimsWhitespace(string input, string expected)
    {
        Assert.Equal(expected, InstalledVersionParsers.ParseGhostscriptVersion(input));
    }

    [Fact]
    public void ParseGhostscriptVersion_ReturnsNull_WhenEmpty()
    {
        Assert.Null(InstalledVersionParsers.ParseGhostscriptVersion("   "));
    }

    [Theory]
    [InlineData("ffmpeg version 6.1.1-3ubuntu5 Copyright (c) 2000-2023 the FFmpeg developers", "6.1.1")]
    [InlineData("ffmpeg version 9.0.2-full_build-www.gyan.dev Copyright (c) 2000-2026", "9.0.2")]
    public void ParseFfmpegVersion_ExtractsLeadingDottedVersion(string firstLine, string expected)
    {
        Assert.Equal(expected, InstalledVersionParsers.ParseFfmpegVersion(firstLine));
    }

    [Fact]
    public void ParseFfmpegVersion_ReturnsNull_WhenLineDoesNotMatch()
    {
        Assert.Null(InstalledVersionParsers.ParseFfmpegVersion("some unrelated output"));
    }

    [Theory]
    [InlineData("Version: ImageMagick 6.9.12-98 Q16 x86_64 18038 https://legacy.imagemagick.org", "6.9.12.98")]
    [InlineData("Version: ImageMagick 7.1.2-31 Q16-HDRI x86_64 [some build info]", "7.1.2.31")]
    public void ParseImageMagickVersion_ExtractsVersionAndNormalizesDashesToDots(string versionOutput, string expected)
    {
        Assert.Equal(expected, InstalledVersionParsers.ParseImageMagickVersion(versionOutput));
    }

    [Fact]
    public void ParseImageMagickVersion_ReturnsNull_WhenOutputDoesNotMatch()
    {
        Assert.Null(InstalledVersionParsers.ParseImageMagickVersion("not imagemagick output"));
    }
}
