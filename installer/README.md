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

1. Publish every project in Release configuration, self-contained and
   single-file for `win-x64`. The `-r`/`--self-contained` flags must be
   passed explicitly on the command line — the project files also declare
   `RuntimeIdentifier`/`SelfContained` (in `Directory.Build.targets`), but
   confirmed on a real Windows build that those settings alone don't
   reliably produce the `win-x64` output folder; only the explicit CLI
   flags do:

   ```
   dotnet publish src\MediaSuite.Manager -c Release -r win-x64 --self-contained true
   dotnet publish src\MediaSuite.Tools.PdfToPng -c Release -r win-x64 --self-contained true
   dotnet publish src\MediaSuite.Tools.MergePdf -c Release -r win-x64 --self-contained true
   dotnet publish src\MediaSuite.Tools.FfmpegConvert -c Release -r win-x64 --self-contained true
   dotnet publish src\MediaSuite.Tools.ReduceForYouTube -c Release -r win-x64 --self-contained true
   dotnet publish src\MediaSuite.Tools.MakePdf -c Release -r win-x64 --self-contained true
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

## Status

The `dotnet publish` step above (with the explicit `-r win-x64
--self-contained true` flags) has been confirmed on a real Windows machine to
produce the `win-x64\publish` output folders the `[Files]` section expects.

Compiling the `.iss` script itself with Inno Setup has not yet been
confirmed — that step is still pending a real test pass. The script was
written carefully against documented Inno Setup 6.x syntax (an attempt to
validate it under Wine in this repo's Linux development environment failed
for unrelated Wine configuration reasons, not anything specific to this
script), but treat it as unverified until it's actually been compiled and
the resulting installer run through end to end.
