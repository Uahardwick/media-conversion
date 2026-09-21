using System.Globalization;
using System.Text.RegularExpressions;
using MediaSuite.Core.Processes;

namespace MediaSuite.Core.Media;

/// <summary>
/// Converts a video to H.264/AAC MP4 via FFmpeg: maps only the first video
/// and (if present) first audio stream, pads odd dimensions to even ones,
/// forces yuv420p, and enables MP4 fast-start. No subtitles, attachments,
/// extra tracks, or HDR tone mapping.
/// </summary>
public sealed class FfmpegVideoConverter
{
    private static readonly Regex DurationLineRegex =
        new(@"Duration:\s*(\d+):(\d+):(\d+(?:\.\d+)?)", RegexOptions.Compiled);

    private static readonly Regex OutTimeLineRegex =
        new(@"^out_time=(\d+):(\d+):(\d+(?:\.\d+)?)", RegexOptions.Compiled);

    private readonly string _ffmpegExecutablePath;

    public FfmpegVideoConverter(string ffmpegExecutablePath)
    {
        _ffmpegExecutablePath = ffmpegExecutablePath;
    }

    public async Task ConvertToMp4Async(
        string inputPath,
        string outputPath,
        string? frameRateArgument,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        TimeSpan? totalDuration = null;
        var errorLines = new List<string>();

        var arguments = new List<string>
        {
            "-y",
            "-i", inputPath,
            "-map", "0:v:0?",
            "-map", "0:a:0?",
            "-c:v", "libx264",
            "-c:a", "aac",
            "-pix_fmt", "yuv420p",
            "-vf", "pad=ceil(iw/2)*2:ceil(ih/2)*2",
        };

        if (!string.IsNullOrEmpty(frameRateArgument))
        {
            arguments.Add("-r");
            arguments.Add(frameRateArgument);
        }

        arguments.Add("-movflags");
        arguments.Add("+faststart");
        arguments.Add("-progress");
        arguments.Add("pipe:1");
        arguments.Add("-nostats");
        arguments.Add(outputPath);

        var result = await ProcessRunner.RunAsync(
            _ffmpegExecutablePath,
            arguments,
            onOutputLine: line =>
            {
                if (progress is null || totalDuration is not { } duration || duration <= TimeSpan.Zero)
                    return;

                var elapsed = TryParseOutTimeLine(line);
                if (elapsed is { } value)
                    progress.Report(Math.Clamp(value.TotalSeconds / duration.TotalSeconds, 0, 1));
            },
            onErrorLine: line =>
            {
                errorLines.Add(line);
                totalDuration ??= TryParseDurationLine(line);
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.ExitCode != 0 || !File.Exists(outputPath))
            throw new FfmpegConversionException(inputPath, string.Join(Environment.NewLine, errorLines.TakeLast(20)));

        progress?.Report(1.0);
    }

    internal static TimeSpan? TryParseDurationLine(string line)
    {
        var match = DurationLineRegex.Match(line);
        return match.Success ? ParseTimestamp(match) : null;
    }

    internal static TimeSpan? TryParseOutTimeLine(string line)
    {
        var match = OutTimeLineRegex.Match(line);
        return match.Success ? ParseTimestamp(match) : null;
    }

    private static TimeSpan ParseTimestamp(Match match)
    {
        var hours = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var minutes = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        var seconds = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
        return TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
    }
}
