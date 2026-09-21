namespace MediaSuite.Core.Processes;

/// <summary>
/// Searches the PATH environment variable for an executable by name.
/// Shared by the per-tool locators (Ghostscript, FFmpeg, ...).
/// </summary>
public static class PathExecutableLocator
{
    public static string? FindOnEnvironmentPath(string executableName)
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
            return null;

        foreach (var directory in pathVariable.Split(Path.PathSeparator))
        {
            if (directory.Length == 0)
                continue;

            var candidate = Path.Combine(directory, executableName);
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }
}
