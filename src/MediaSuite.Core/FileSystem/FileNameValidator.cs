namespace MediaSuite.Core.FileSystem;

/// <summary>
/// Validates a candidate file or folder name against Windows naming rules,
/// independent of the host OS this runs on.
/// </summary>
public static class FileNameValidator
{
    private static readonly char[] InvalidChars = BuildInvalidChars();

    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
    };

    public static FileNameValidationResult Validate(string name, int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(name))
            return FileNameValidationResult.Fail("Name cannot be empty.");

        if (name.Length > maxLength)
            return FileNameValidationResult.Fail($"Name cannot be longer than {maxLength} characters.");

        if (name.IndexOfAny(InvalidChars) >= 0)
            return FileNameValidationResult.Fail("Name contains a character that isn't allowed in Windows file names (\\ / : * ? \" < > |).");

        if (name[^1] is ' ' or '.')
            return FileNameValidationResult.Fail("Name cannot end with a space or a period.");

        var stem = Path.GetFileNameWithoutExtension(name);
        if (ReservedNames.Contains(stem))
            return FileNameValidationResult.Fail($"\"{stem}\" is a reserved Windows device name and cannot be used.");

        return FileNameValidationResult.Success();
    }

    private static char[] BuildInvalidChars()
    {
        var chars = new List<char>(Enumerable.Range(0, 32).Select(c => (char)c))
        {
            '<', '>', ':', '"', '/', '\\', '|', '?', '*',
        };
        return chars.ToArray();
    }
}

public readonly record struct FileNameValidationResult(bool IsValid, string? ErrorMessage)
{
    public static FileNameValidationResult Success() => new(true, null);

    public static FileNameValidationResult Fail(string message) => new(false, message);
}
