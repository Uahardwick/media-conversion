# Building the installer

This folder holds the Inno Setup script that packages the Manager and all
five right-click tools into one `MediaToolsSuiteSetup.exe`. It must be built
on Windows — both the `dotnet publish` step and the Inno Setup compiler need
Windows.

## Prerequisites

- .NET 8 SDK
- [Inno Setup](https://jrsoftware.org/isinfo.php) 6.x (developed against 6.7.3;
  binaries are also published at https://github.com/jrsoftware/issrc/releases)

## Steps

1. Publish every project in Release configuration (each one self-contained,
   single-file, `win-x64`, per the settings in the repo's
   `Directory.Build.props`):

   ```
   dotnet publish src\MediaSuite.Manager -c Release
   dotnet publish src\MediaSuite.Tools.PdfToPng -c Release
   dotnet publish src\MediaSuite.Tools.MergePdf -c Release
   dotnet publish src\MediaSuite.Tools.FfmpegConvert -c Release
   dotnet publish src\MediaSuite.Tools.ReduceForYouTube -c Release
   dotnet publish src\MediaSuite.Tools.MakePdf -c Release
   ```

2. Open `installer\MediaToolsSuite.iss` in the Inno Setup Compiler (or run
   `ISCC.exe installer\MediaToolsSuite.iss` from this folder) and build.

3. The output lands in `installer\Output\MediaToolsSuiteSetup.exe`
   (git-ignored).

## What it does

- Copies all six publish outputs flat into one folder
  (`%ProgramFiles%\Media Tools Suite` by default).
- Creates a Start Menu shortcut (and an optional desktop shortcut) for the
  Manager.
- Silently runs `MediaSuiteManager.exe --register-tools` as part of the
  install, so all five right-click verbs are live immediately — no need to
  open the Manager first.
- Offers to open the Manager on the finish page (unchecked by default) so
  the user can install FFmpeg, Ghostscript, and ImageMagick right away.
- On uninstall, runs `MediaSuiteManager.exe --unregister-tools` before
  removing files, then shows a message clarifying that FFmpeg, Ghostscript,
  and ImageMagick are deliberately left installed.

## Not verified here

This `.iss` file has not been compiled or run. The development environment
for this repository is Linux, and Inno Setup itself only runs on Windows (an
attempt to run it under Wine in that environment failed for unrelated Wine
configuration reasons, not anything specific to this script). The script was
written carefully against documented Inno Setup 6.x syntax, but it needs a
real build-and-install pass on Windows before being trusted — in particular,
double-check that the `dotnet publish` output paths referenced in `[Files]`
match what your SDK version actually produces.
