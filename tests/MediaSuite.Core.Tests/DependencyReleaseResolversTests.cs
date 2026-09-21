using MediaSuite.Core.Updates;
using Xunit;

namespace MediaSuite.Core.Tests;

public class DependencyReleaseResolversTests
{
    [Fact]
    public void ResolveGhostscript_PicksWin64Installer_AndUsesReleaseName()
    {
        var release = new GitHubRelease
        {
            TagName = "gs10080",
            Name = "10.08.0",
            Assets =
            [
                new GitHubReleaseAsset { Name = "gs10080w32.exe", BrowserDownloadUrl = "https://example.com/gs10080w32.exe" },
                new GitHubReleaseAsset { Name = "gs10080w64.exe", BrowserDownloadUrl = "https://example.com/gs10080w64.exe" },
                new GitHubReleaseAsset { Name = "ghostpdl-10.08.0.tar.gz", BrowserDownloadUrl = "https://example.com/src.tar.gz" },
            ],
        };

        var resolved = DependencyReleaseResolvers.ResolveGhostscript(release);

        Assert.Equal("10.08.0", resolved.Version);
        Assert.Equal("https://example.com/gs10080w64.exe", resolved.DownloadUrl);
    }

    [Fact]
    public void ResolveGhostscript_FallsBackToTag_WhenNameMissing()
    {
        var release = new GitHubRelease
        {
            TagName = "gs10080",
            Name = null,
            Assets = [new GitHubReleaseAsset { Name = "gs10080w64.exe", BrowserDownloadUrl = "https://example.com/gs10080w64.exe" }],
        };

        var resolved = DependencyReleaseResolvers.ResolveGhostscript(release);

        Assert.Equal("10080", resolved.Version);
    }

    [Fact]
    public void ResolveGhostscript_Throws_WhenNoWin64AssetPresent()
    {
        var release = new GitHubRelease
        {
            TagName = "gs10080",
            Assets = [new GitHubReleaseAsset { Name = "gs10080w32.exe", BrowserDownloadUrl = "https://example.com/gs10080w32.exe" }],
        };

        Assert.Throws<InvalidOperationException>(() => DependencyReleaseResolvers.ResolveGhostscript(release));
    }

    [Fact]
    public void ResolveFfmpeg_PicksFullBuildZip_AndUsesTagAsVersion()
    {
        var release = new GitHubRelease
        {
            TagName = "9.0.2",
            Assets =
            [
                new GitHubReleaseAsset { Name = "ffmpeg-9.0.2-essentials_build.zip", BrowserDownloadUrl = "https://example.com/essentials.zip" },
                new GitHubReleaseAsset { Name = "ffmpeg-9.0.2-full_build.zip", BrowserDownloadUrl = "https://example.com/full.zip" },
                new GitHubReleaseAsset { Name = "ffmpeg-9.0.2-full_build-shared.zip", BrowserDownloadUrl = "https://example.com/shared.zip" },
            ],
        };

        var resolved = DependencyReleaseResolvers.ResolveFfmpeg(release);

        Assert.Equal("9.0.2", resolved.Version);
        Assert.Equal("https://example.com/full.zip", resolved.DownloadUrl);
    }

    [Fact]
    public void ResolveImageMagick_PicksX64DllInstaller_NotX86OrArm64()
    {
        var release = new GitHubRelease
        {
            TagName = "7.1.2-31",
            Assets =
            [
                new GitHubReleaseAsset { Name = "ImageMagick-7.1.2-31-Q16-HDRI-x86-dll.exe", BrowserDownloadUrl = "https://example.com/x86.exe" },
                new GitHubReleaseAsset { Name = "ImageMagick-7.1.2-31-Q16-HDRI-x64-dll.exe", BrowserDownloadUrl = "https://example.com/x64.exe" },
                new GitHubReleaseAsset { Name = "ImageMagick-7.1.2-31-Q16-HDRI-arm64-dll.exe", BrowserDownloadUrl = "https://example.com/arm64.exe" },
            ],
        };

        var resolved = DependencyReleaseResolvers.ResolveImageMagick(release);

        Assert.Equal("7.1.2.31", resolved.Version);
        Assert.Equal("https://example.com/x64.exe", resolved.DownloadUrl);
    }
}
