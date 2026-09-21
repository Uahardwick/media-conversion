# Media Tools Suite

Windows 10/11 installer and set of Explorer right-click tools for a media/church multimedia team: PDF↔PNG/merge, video conversion via FFmpeg, image reduction for YouTube, and Word/PowerPoint to PDF. Built on shared open-source tools (FFmpeg, Ghostscript, ImageMagick) that the suite installs and keeps updated.

## Layout

- `src/MediaSuite.Core` — shared library: Windows filename validation, page-range parsing, unique path numbering, external-process running. Cross-platform (`net8.0`) so it can be unit tested anywhere.
- `src/MediaSuite.Manager` — the WPF app that is both the initial installer's payload and the permanent "check for updates / install / remove" dashboard. No background service or scheduled polling — checks only run when the user clicks **Check for Updates**.
- `src/MediaSuite.Tools.*` — one small WinForms exe per Explorer right-click command:
  - `PdfToPng` — "Convert PDF to PNG"
  - `MergePdf` — "Merge PDFs"
  - `FfmpegConvert` — "Convert with FFMPEG"
  - `ReduceForYouTube` — "Reduce for YouTube PNG (under 2 MB)"
  - `OfficeToPdf` — Word/PowerPoint → PDF via Office COM automation
- `tests/MediaSuite.Core.Tests` — xUnit tests for `MediaSuite.Core`.

Shared runtime dependencies (FFmpeg, Ghostscript, ImageMagick) are tracked and installed/updated by the Manager into their normal Windows locations. NDI Tools is out of scope for this installer.

## Status

Scaffolding stage: `MediaSuite.Core` has real logic (validation, page-range parsing, unique-path numbering, process running) with tests. The Manager and the five tool projects are placeholder stubs pending implementation.

## Building

Requires the .NET 8 SDK. The tool/Manager projects target `net8.0-windows` (WinForms/WPF) and can only be built and run on Windows; `MediaSuite.Core` and its tests target plain `net8.0` and build cross-platform.

```
dotnet build MediaSuite.sln
dotnet test tests/MediaSuite.Core.Tests
```
