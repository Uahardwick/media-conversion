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

All five right-click tools (`PdfToPng`, `MergePdf`, `FfmpegConvert`, `ReduceForYouTube`, `MakePdf`) and the Manager are implemented.

The Manager tracks FFmpeg, Ghostscript, and ImageMagick as shared dependencies, each installed/updated/removed via its real GitHub release (`ArtifexSoftware/ghostpdl-downloads`, `GyanD/codexffmpeg`, `ImageMagick/ImageMagick` respectively — all three confirmed to publish genuine Windows binaries there, not just source). It shows Installed/Latest/Status per component with manual-only "Check for Updates" (no polling), and registers/removes each tool's own Explorer right-click verb independently. The five tools themselves ship bundled with the Manager as one unit (there's no per-tool release pipeline for a suite this size), so "Install"/"Remove" for a tool just toggles its context-menu registration rather than downloading anything.

None of the WinForms/WPF UI code has been compiled or run on Windows — this has all been developed from a Linux environment, where `net8.0-windows` projects cannot be built at all (confirmed: even with the .NET 8 SDK installed, the required `Microsoft.NET.Sdk.WindowsDesktop` MSBuild SDK isn't available on Linux). Every tool's UI, and the Manager's registry/installer/PATH-manipulation code in particular, needs a real click-through on Windows before being trusted. `MakePdf`'s Office COM automation is the least-verified piece of all, since this environment has no Office and no COM subsystem whatsoever.

## Building

Requires the .NET 8 SDK. The tool/Manager projects target `net8.0-windows` (WinForms/WPF) and can only be built and run on Windows; `MediaSuite.Core` and its tests target plain `net8.0` and build cross-platform.

```
dotnet build MediaSuite.sln
dotnet test tests/MediaSuite.Core.Tests
```
