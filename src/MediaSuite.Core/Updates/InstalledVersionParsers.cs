using System.Text.RegularExpressions;

namespace MediaSuite.Core.Updates;

/// <summary>
/// Extracts a clean version string from each dependency's own "--version"
/// style output, normalized to match the dotted form
/// <see cref="DependencyReleaseResolvers"/> derives from GitHub releases,
/// so installed-vs-latest comparisons are apples to apples.
/// </summary>
public static class InstalledVersionParsers
{
    private static readonly Regex FfmpegVersionPattern = new(@"ffmpeg version\s+(\d+(?:\.\d+)+)", RegexOptions.IgnoreCase);
    private static readonly Regex ImageMagickVersionPattern = new(@"ImageMagick\s+(\d+(?:[.-]\d+)+)", RegexOptions.IgnoreCase);

    public static string? ParseGhostscriptVersion(string versionOutput)
    {
        var trimmed = versionOutput.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static string? ParseFfmpegVersion(string firstLine)
    {
        var match = FfmpegVersionPattern.Match(firstLine);
        return match.Success ? match.Groups[1].Value : null;
    }

    public static string? ParseImageMagickVersion(string versionOutput)
    {
        var match = ImageMagickVersionPattern.Match(versionOutput);
        return match.Success ? match.Groups[1].Value.Replace('-', '.') : null;
    }
}
