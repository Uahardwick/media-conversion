using MediaSuite.Core.Processes;

namespace MediaSuite.Core.Pdf;

/// <summary>
/// Finds the Ghostscript console executable on this machine. Ghostscript's
/// own installer does not add itself to PATH, so this also probes its
/// default per-machine install location.
/// </summary>
public static class GhostscriptLocator
{
    private const string ExecutableName = "gswin64c.exe";

    public static string? Find() =>
        PathExecutableLocator.FindOnEnvironmentPath(ExecutableName) ?? FindInDefaultInstallLocation();

    private static string? FindInDefaultInstallLocation()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (string.IsNullOrEmpty(programFiles))
            return null;

        var gsRoot = Path.Combine(programFiles, "gs");
        if (!Directory.Exists(gsRoot))
            return null;

        // Ghostscript installs into a versioned folder, e.g. "gs10.03.0"; pick the newest.
        var newestVersionFolder = Directory.GetDirectories(gsRoot, "gs*")
            .Select(Path.GetFileName)
            .OfType<string>()
            .Select(name => (Name: name, Version: ParseVersion(name)))
            .OrderByDescending(entry => entry.Version)
            .Select(entry => entry.Name)
            .FirstOrDefault();

        if (newestVersionFolder is null)
            return null;

        var candidate = Path.Combine(gsRoot, newestVersionFolder, "bin", ExecutableName);
        return File.Exists(candidate) ? candidate : null;
    }

    internal static Version ParseVersion(string folderName)
    {
        var digits = folderName.Length > 2 ? folderName[2..] : string.Empty;
        return Version.TryParse(digits, out var version) ? version : new Version(0, 0);
    }
}
