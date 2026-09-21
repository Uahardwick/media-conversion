using System.Text.RegularExpressions;
using MediaSuite.Core.Processes;

namespace MediaSuite.Core.Media;

/// <summary>
/// Finds the ImageMagick (v7) "magick" executable on this machine. Its
/// installer does not add itself to PATH, so this also probes the
/// versioned per-machine install folder it creates directly under
/// Program Files (e.g. "ImageMagick-7.1.1-Q16-HDRI").
/// </summary>
public static class ImageMagickLocator
{
    private const string ExecutableName = "magick.exe";

    private static readonly Regex VersionRegex = new(@"\d+(\.\d+)+", RegexOptions.Compiled);

    public static string? Find() =>
        PathExecutableLocator.FindOnEnvironmentPath(ExecutableName) ?? FindInDefaultInstallLocation();

    private static string? FindInDefaultInstallLocation()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (string.IsNullOrEmpty(programFiles) || !Directory.Exists(programFiles))
            return null;

        var newestVersionFolder = Directory.GetDirectories(programFiles, "ImageMagick-*")
            .Select(Path.GetFileName)
            .OfType<string>()
            .Select(name => (Name: name, Version: ParseVersion(name)))
            .OrderByDescending(entry => entry.Version)
            .Select(entry => entry.Name)
            .FirstOrDefault();

        if (newestVersionFolder is null)
            return null;

        var candidate = Path.Combine(programFiles, newestVersionFolder, ExecutableName);
        return File.Exists(candidate) ? candidate : null;
    }

    internal static Version ParseVersion(string folderName)
    {
        var match = VersionRegex.Match(folderName);
        return match.Success && Version.TryParse(match.Value, out var version) ? version : new Version(0, 0);
    }
}
