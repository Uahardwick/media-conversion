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
  - `MakePdf` — "Make PDF" (Word/PowerPoint → PDF via Office COM automation)
- `tests/MediaSuite.Core.Tests` — xUnit tests for `MediaSuite.Core`.

Shared runtime dependencies (FFmpeg, Ghostscript, ImageMagick) are tracked and installed/updated by the Manager into their normal Windows locations. NDI Tools is out of scope for this installer.

## Status

`PdfToPng`, `MergePdf`, `FfmpegConvert`, `ReduceForYouTube`, and `MakePdf` are implemented, each backed by real logic in `MediaSuite.Core` (Ghostscript, FFmpeg, and ImageMagick wrappers) with unit tests plus scratch end-to-end verification against real installs of those tools where possible. `MediaSuite.Manager` (the install/update dashboard) is still a placeholder stub.

None of the WinForms/WPF UI code has been compiled or run on Windows yet — this has all been developed and tested from a Linux environment, where `net8.0-windows` projects cannot be built at all. Each tool's UI needs a real click-through on Windows before being trusted, and `MakePdf`'s Office COM automation in particular is entirely unverified since it depends on Word/PowerPoint being installed, which has no equivalent in this environment even for partial testing.

## Building

Requires the .NET 8 SDK. The tool/Manager projects target `net8.0-windows` (WinForms/WPF) and can only be built and run on Windows; `MediaSuite.Core` and its tests target plain `net8.0` and build cross-platform.

```
dotnet build MediaSuite.sln
dotnet test tests/MediaSuite.Core.Tests
```
