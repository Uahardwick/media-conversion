namespace MediaSuite.Core.FileSystem;

/// <summary>
/// Finds the first available path from a caller-supplied numbering scheme.
/// Different tools number collisions differently (e.g. "Name (2)" for folders
/// vs. "Name (1)-Suffix.png" for files), so the numbering itself is left to
/// the caller via <paramref name="candidatePath"/>; this only walks attempts
/// until one doesn't already exist.
/// </summary>
public static class UniquePathGenerator
{
    public static string FindAvailable(Func<int, string> candidatePath, Func<string, bool> exists)
    {
        for (var attempt = 0; ; attempt++)
        {
            var candidate = candidatePath(attempt);
            if (!exists(candidate))
                return candidate;
        }
    }
}
