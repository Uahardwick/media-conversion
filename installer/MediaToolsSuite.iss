; Media Tools Suite bootstrap installer.
;
; This is a one-time bootstrapper: it copies the Manager and all five
; right-click tools (each published self-contained, single-file, win-x64)
; into one install folder, creates a Start Menu shortcut, and registers
; every tool's Explorer right-click verb immediately so the suite works
; without the user having to open the Manager first. All later
; install/update/remove of FFmpeg, Ghostscript, and ImageMagick is done
; through the Manager itself, not here - this installer never touches
; those three.
;
; Build with Inno Setup 6.x (tested against 6.7.3) on Windows, after
; publishing every project in Release configuration:
;   dotnet publish src\MediaSuite.Manager -c Release
;   dotnet publish src\MediaSuite.Tools.PdfToPng -c Release
;   dotnet publish src\MediaSuite.Tools.MergePdf -c Release
;   dotnet publish src\MediaSuite.Tools.FfmpegConvert -c Release
;   dotnet publish src\MediaSuite.Tools.ReduceForYouTube -c Release
;   dotnet publish src\MediaSuite.Tools.MakePdf -c Release
; then open this .iss in the Inno Setup Compiler (or run ISCC.exe against
; it) from this "installer" folder.

#define MyAppName "Media Tools Suite"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "UaHardwick"
#define MyAppExeName "MediaSuiteManager.exe"
#define PublishRoot(str Project) "..\src\" + Project + "\bin\Release\net8.0-windows\win-x64\publish\*"

[Setup]
; Keep this GUID fixed across versions so upgrades replace the existing
; install instead of creating a second "Media Tools Suite" entry.
AppId={{4685DA0E-F9E9-48C5-B862-256826B8915D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=MediaToolsSuiteSetup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#PublishRoot("MediaSuite.Manager")}"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#PublishRoot("MediaSuite.Tools.PdfToPng")}"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#PublishRoot("MediaSuite.Tools.MergePdf")}"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#PublishRoot("MediaSuite.Tools.FfmpegConvert")}"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#PublishRoot("MediaSuite.Tools.ReduceForYouTube")}"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#PublishRoot("MediaSuite.Tools.MakePdf")}"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Runs silently, elevated (inherited from this already-elevated installer),
; as part of the install itself - no checkbox, no user interaction.
Filename: "{app}\{#MyAppExeName}"; Parameters: "--register-tools"; Flags: runhidden waituntilterminated; StatusMsg: "Setting up right-click menus..."
; Offered on the finish page; unchecked by default so setup doesn't force
; the dashboard open.
Filename: "{app}\{#MyAppExeName}"; Description: "Open Media Tools Suite to install FFmpeg, Ghostscript, and ImageMagick"; Flags: postinstall nowait skipifsilent unchecked

[UninstallRun]
; Must run before Inno deletes {app}, since it needs the exe to still be
; there; RunOnceId is required by Inno for [UninstallRun] entries.
Filename: "{app}\{#MyAppExeName}"; Parameters: "--unregister-tools"; Flags: runhidden waituntilterminated; RunOnceId: "UnregisterTools"

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    MsgBox(
      'Media Tools Suite has been removed. FFmpeg, Ghostscript, and ImageMagick were left installed, since they''re shared tools other programs may also use.' + #13#10 + #13#10 +
      'To remove them too, uninstall them individually from Windows Settings > Apps.',
      mbInformation, MB_OK);
  end;
end;
