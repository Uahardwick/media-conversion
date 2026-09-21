using Microsoft.Win32;

namespace MediaSuite.Manager.Shell;

/// <summary>
/// Registers/removes classic Explorer right-click verbs under
/// HKEY_CLASSES_ROOT\SystemFileAssociations\{ext}\shell. On Windows 11
/// these appear under "Show more options" rather than the trimmed
/// top-level menu; that tradeoff was accepted when this was designed.
/// </summary>
internal static class ContextMenuRegistrar
{
    private const string VerbKeyPrefix = "MediaSuite.";

    public static void Register(string toolId, string menuText, string executablePath, IReadOnlyList<string> extensions)
    {
        foreach (var extension in extensions)
        {
            var basePath = $@"SystemFileAssociations\{extension}\shell\{VerbKeyPrefix}{toolId}";
            using var shellKey = Registry.ClassesRoot.CreateSubKey(basePath)
                ?? throw new InvalidOperationException($"Couldn't create the registry key for \"{extension}\".");

            shellKey.SetValue(null, menuText);
            shellKey.SetValue("Icon", $"\"{executablePath}\",0");

            using var commandKey = shellKey.CreateSubKey("command")
                ?? throw new InvalidOperationException($"Couldn't create the command registry key for \"{extension}\".");
            commandKey.SetValue(null, $"\"{executablePath}\" \"%1\"");
        }
    }

    public static void Unregister(string toolId, IReadOnlyList<string> extensions)
    {
        foreach (var extension in extensions)
        {
            var basePath = $@"SystemFileAssociations\{extension}\shell\{VerbKeyPrefix}{toolId}";
            Registry.ClassesRoot.DeleteSubKeyTree(basePath, throwOnMissingSubKey: false);
        }
    }

    public static bool IsRegistered(string toolId, string extension)
    {
        var basePath = $@"SystemFileAssociations\{extension}\shell\{VerbKeyPrefix}{toolId}";
        using var key = Registry.ClassesRoot.OpenSubKey(basePath);
        return key is not null;
    }
}
