namespace MediaSuite.Core.Updates;

/// <summary>
/// Compares dotted/dashed version strings numerically, segment by segment,
/// so "10.08.0" correctly compares against "10.2.1" (lexicographic string
/// comparison would get this wrong).
/// </summary>
public static class DottedVersionComparer
{
    public static bool IsNewer(string latest, string installed)
    {
        var latestParts = ParseParts(latest);
        var installedParts = ParseParts(installed);

        var length = Math.Max(latestParts.Length, installedParts.Length);
        for (var i = 0; i < length; i++)
        {
            var latestPart = i < latestParts.Length ? latestParts[i] : 0;
            var installedPart = i < installedParts.Length ? installedParts[i] : 0;
            if (latestPart != installedPart)
                return latestPart > installedPart;
        }

        return false;
    }

    private static int[] ParseParts(string version) =>
        version.Split('.', '-')
            .Select(part => int.TryParse(part, out var value) ? value : 0)
            .ToArray();
}
