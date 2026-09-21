using System.Diagnostics;

namespace MediaSuite.Core.Processes;

/// <summary>
/// Runs an external tool (ffmpeg, gswin64c, magick, soffice, ...) asynchronously,
/// capturing output line-by-line and killing the whole process tree on cancellation.
/// </summary>
public static class ProcessRunner
{
    public static async Task<ProcessResult> RunAsync(
        string fileName,
        IEnumerable<string> arguments,
        Action<string>? onOutputLine = null,
        Action<string>? onErrorLine = null,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

        var output = new List<string>();
        var error = new List<string>();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null)
                return;

            output.Add(e.Data);
            onOutputLine?.Invoke(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null)
                return;

            error.Add(e.Data);
            onErrorLine?.Invoke(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var cancellationRegistration = cancellationToken.Register(() => TryKill(process));

        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = output,
            StandardError = error,
        };
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // Process already exited between the check and the kill request.
        }
    }
}

public sealed class ProcessResult
{
    public required int ExitCode { get; init; }

    public required IReadOnlyList<string> StandardOutput { get; init; }

    public required IReadOnlyList<string> StandardError { get; init; }
}
