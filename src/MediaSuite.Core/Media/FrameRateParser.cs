using System.Globalization;

namespace MediaSuite.Core.Media;

/// <summary>
/// Validates a user-entered frame rate, accepting both decimal
/// ("29.97") and fractional ("30000/1001") notation. The validated
/// string is passed straight through to ffmpeg's -r option, which
/// understands both forms natively.
/// </summary>
public static class FrameRateParser
{
    public static FrameRateParseResult Parse(string input)
    {
        var trimmed = input.Trim();
        if (trimmed.Length == 0)
            return FrameRateParseResult.Fail("Enter a frame rate, such as 30 or 29.97.");

        var slash = trimmed.IndexOf('/');
        if (slash >= 0)
        {
            var numeratorText = trimmed[..slash].Trim();
            var denominatorText = trimmed[(slash + 1)..].Trim();
            if (!TryParsePositive(numeratorText, out var numerator) || !TryParsePositive(denominatorText, out var denominator))
                return FrameRateParseResult.Fail($"\"{trimmed}\" isn't a valid frame rate.");

            return FrameRateParseResult.Success(trimmed, numerator / denominator);
        }

        if (!TryParsePositive(trimmed, out var value))
            return FrameRateParseResult.Fail($"\"{trimmed}\" isn't a valid frame rate.");

        return FrameRateParseResult.Success(trimmed, value);
    }

    private static bool TryParsePositive(string text, out double value)
    {
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && value > 0)
            return true;

        value = 0;
        return false;
    }
}

public sealed class FrameRateParseResult
{
    private FrameRateParseResult(bool isValid, string? errorMessage, string ffmpegArgument, double framesPerSecond)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        FfmpegArgument = ffmpegArgument;
        FramesPerSecond = framesPerSecond;
    }

    public bool IsValid { get; }

    public string? ErrorMessage { get; }

    public string FfmpegArgument { get; }

    public double FramesPerSecond { get; }

    public static FrameRateParseResult Success(string ffmpegArgument, double framesPerSecond) =>
        new(true, null, ffmpegArgument, framesPerSecond);

    public static FrameRateParseResult Fail(string message) => new(false, message, string.Empty, 0);
}
