namespace MediaSuite.Manager.Tools;

internal sealed record ToolDefinition(
    string Id,
    string DisplayName,
    string MenuText,
    string ExecutableFileName,
    IReadOnlyList<string> Extensions);

/// <summary>
/// All five right-click tools ship bundled with the Manager itself (same
/// install, same version) rather than being independently downloadable -
/// there's no per-tool release/CI pipeline for this small suite. "Install"
/// and "Remove" here only toggle each tool's Explorer context-menu
/// registration; the executables are always present once the suite itself
/// is installed.
/// </summary>
internal static class ToolCatalog
{
    public static readonly IReadOnlyList<ToolDefinition> All =
    [
        new ToolDefinition("PdfToPng", "PDF to PNG", "Convert PDF to PNG", "PdfToPng.exe", [".pdf"]),
        new ToolDefinition("MergePdf", "Merge PDF", "Merge PDFs", "MergePdf.exe", [".pdf"]),
        new ToolDefinition(
            "FfmpegConvert",
            "Convert with FFMPEG",
            "Convert with FFMPEG",
            "FfmpegConvert.exe",
            [
                ".mp4", ".mov", ".avi", ".mkv", ".webm", ".m4v", ".wmv", ".mpg", ".mpeg", ".mpe", ".m2v",
                ".mts", ".m2ts", ".ts", ".flv", ".f4v", ".3gp", ".3g2", ".vob", ".ogv", ".asf", ".divx", ".mxf", ".qt",
            ]),
        new ToolDefinition(
            "ReduceForYouTube",
            "Reduce for YouTube",
            "Reduce for YouTube PNG (under 2 MB)",
            "ReduceForYouTube.exe",
            [".jpg", ".jpeg", ".png"]),
        new ToolDefinition("MakePdf", "Make PDF", "Make PDF", "MakePdf.exe", [".doc", ".docx", ".ppt", ".pptx"]),
    ];
}
