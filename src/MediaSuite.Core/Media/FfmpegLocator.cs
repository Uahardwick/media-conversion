using MediaSuite.Core.Processes;

namespace MediaSuite.Core.Media;

/// <summary>
/// Finds the FFmpeg executable on this machine. FFmpeg has no official
/// Windows installer, so the suite extracts it into a fixed default
/// location; this checks PATH first in case that's changed.
/// </summary>
public static class FfmpegLocator
{
    private const string ExecutableName = "ffmpeg.exe";

    public static string? Find() =>
        PathExecutableLocator.FindOnEnvironmentPath(ExecutableName) ?? FindInDefaultInstallLocation();

    private static string? FindInDefaultInstallLocation()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (string.IsNullOrEmpty(programFiles))
            return null;

        var candidate = Path.Combine(programFiles, "FFmpeg", "bin", ExecutableName);
        return File.Exists(candidate) ? candidate : null;
    }
}
