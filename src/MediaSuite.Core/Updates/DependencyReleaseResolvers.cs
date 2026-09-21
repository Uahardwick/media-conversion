using System.Text.RegularExpressions;

namespace MediaSuite.Core.Updates;

public sealed record ResolvedRelease(string Version, string DownloadUrl);

/// <summary>
/// Picks the right Windows asset and derives a display version out of a
/// GitHub release for each of the suite's three shared dependencies. All
/// three are confirmed to publish real Windows binaries as GitHub release
/// assets, despite none of them being "GitHub-native" projects in the way
/// Ghostscript's own downloads repo is.
/// </summary>
public static class DependencyReleaseResolvers
{
    private static readonly Regex GhostscriptAssetPattern = new(@"^gs\d+w64\.exe$", RegexOptions.IgnoreCase);
    private static readonly Regex FfmpegAssetPattern = new(@"^ffmpeg-.*-full_build\.zip$", RegexOptions.IgnoreCase);
    private static readonly Regex ImageMagickAssetPattern = new(@"^ImageMagick-.*-Q16-HDRI-x64-dll\.exe$", RegexOptions.IgnoreCase);

    public static ResolvedRelease ResolveGhostscript(GitHubRelease release)
    {
        var asset = release.Assets.FirstOrDefault(a => GhostscriptAssetPattern.IsMatch(a.Name))
            ?? throw new InvalidOperationException("Couldn't find a 64-bit Windows installer in the latest Ghostscript release.");

        var version = !string.IsNullOrWhiteSpace(release.Name) ? release.Name.Trim() : release.TagName.TrimStart('g', 's');
        return new ResolvedRelease(version, asset.BrowserDownloadUrl);
    }

    public static ResolvedRelease ResolveFfmpeg(GitHubRelease release)
    {
        var asset = release.Assets.FirstOrDefault(a => FfmpegAssetPattern.IsMatch(a.Name))
            ?? throw new InvalidOperationException("Couldn't find a Windows build in the latest FFmpeg release.");

        return new ResolvedRelease(release.TagName.Trim(), asset.BrowserDownloadUrl);
    }

    public static ResolvedRelease ResolveImageMagick(GitHubRelease release)
    {
        var asset = release.Assets.FirstOrDefault(a => ImageMagickAssetPattern.IsMatch(a.Name))
            ?? throw new InvalidOperationException("Couldn't find a 64-bit Windows installer in the latest ImageMagick release.");

        var version = release.TagName.Trim().Replace('-', '.');
        return new ResolvedRelease(version, asset.BrowserDownloadUrl);
    }
}
