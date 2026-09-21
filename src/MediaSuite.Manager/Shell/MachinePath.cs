namespace MediaSuite.Manager.Shell;

/// <summary>
/// Adds/removes a directory from the machine-wide PATH environment
/// variable, for dependencies (FFmpeg) that ship as a plain archive rather
/// than an installer that registers itself.
/// </summary>
internal static class MachinePath
{
    public static void EnsureDirectoryOnPath(string directory)
    {
        var current = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? string.Empty;
        var entries = current.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (entries.Any(entry => PathsEqual(entry, directory)))
            return;

        var updated = current.Length > 0 && !current.EndsWith(Path.PathSeparator)
            ? $"{current}{Path.PathSeparator}{directory}"
            : $"{current}{directory}";

        Environment.SetEnvironmentVariable("PATH", updated, EnvironmentVariableTarget.Machine);

        // Also update this process's own PATH so a locator check succeeds
        // immediately, without requiring the Manager to restart.
        var processPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        Environment.SetEnvironmentVariable("PATH", $"{processPath}{Path.PathSeparator}{directory}");
    }

    public static void RemoveDirectoryFromPath(string directory)
    {
        var current = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? string.Empty;
        var remaining = current
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(entry => !PathsEqual(entry, directory));

        Environment.SetEnvironmentVariable("PATH", string.Join(Path.PathSeparator, remaining), EnvironmentVariableTarget.Machine);
    }

    private static bool PathsEqual(string a, string b) =>
        string.Equals(a.TrimEnd('\\'), b.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);
}
