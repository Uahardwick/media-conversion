using MediaSuite.Core.Processes;
using Microsoft.Win32;

namespace MediaSuite.Manager.Shell;

/// <summary>
/// Finds a program's own uninstaller by matching DisplayName in the
/// standard Windows Uninstall registry keys, then runs it with the given
/// silent switches. Used for dependencies whose installer doesn't hand us
/// a predictable uninstaller path directly (Ghostscript, ImageMagick).
/// </summary>
internal static class UninstallRegistryHelper
{
    private static readonly string[] UninstallRoots =
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
    };

    public static async Task RunUninstallerAsync(string displayNameSubstring, string silentArguments, CancellationToken cancellationToken)
    {
        var uninstallString = FindUninstallString(displayNameSubstring)
            ?? throw new InvalidOperationException($"Couldn't find an installed program matching \"{displayNameSubstring}\" to uninstall.");

        var (fileName, baseArguments) = SplitCommandLine(uninstallString);
        var arguments = string.IsNullOrEmpty(baseArguments)
            ? new[] { silentArguments }
            : new[] { baseArguments, silentArguments };

        var result = await ProcessRunner.RunAsync(fileName, arguments, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"Uninstaller for \"{displayNameSubstring}\" exited with code {result.ExitCode}.");
    }

    private static string? FindUninstallString(string displayNameSubstring)
    {
        foreach (var root in UninstallRoots)
        {
            using var rootKey = Registry.LocalMachine.OpenSubKey(root);
            if (rootKey is null)
                continue;

            foreach (var subKeyName in rootKey.GetSubKeyNames())
            {
                using var subKey = rootKey.OpenSubKey(subKeyName);
                var displayName = subKey?.GetValue("DisplayName") as string;
                if (displayName is not null && displayName.Contains(displayNameSubstring, StringComparison.OrdinalIgnoreCase))
                    return subKey?.GetValue("UninstallString") as string;
            }
        }

        return null;
    }

    private static (string FileName, string Arguments) SplitCommandLine(string commandLine)
    {
        var trimmed = commandLine.Trim();
        if (trimmed.StartsWith('"'))
        {
            var closingQuote = trimmed.IndexOf('"', 1);
            if (closingQuote > 0)
                return (trimmed[1..closingQuote], trimmed[(closingQuote + 1)..].Trim());
        }

        var firstSpace = trimmed.IndexOf(' ');
        return firstSpace < 0 ? (trimmed, string.Empty) : (trimmed[..firstSpace], trimmed[(firstSpace + 1)..].Trim());
    }
}
