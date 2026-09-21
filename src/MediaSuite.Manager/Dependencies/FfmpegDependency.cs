using System.IO.Compression;
using System.Net.Http;
using MediaSuite.Core.Media;
using MediaSuite.Core.Processes;
using MediaSuite.Core.Updates;
using MediaSuite.Manager.Shell;

namespace MediaSuite.Manager.Dependencies;

internal sealed class FfmpegDependency : IManagedDependency
{
    private static readonly string InstallDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "FFmpeg");

    public string Id => "ffmpeg";

    public string DisplayName => "FFmpeg";

    public async Task<string?> GetInstalledVersionAsync(CancellationToken cancellationToken)
    {
        var path = FfmpegLocator.Find();
        if (path is null)
            return null;

        var output = new List<string>();
        await ProcessRunner.RunAsync(path, new[] { "-version" }, onOutputLine: output.Add, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return output.Count > 0 ? InstalledVersionParsers.ParseFfmpegVersion(output[0]) : null;
    }

    public async Task<ResolvedRelease> GetLatestReleaseAsync(HttpClient httpClient, CancellationToken cancellationToken)
    {
        var client = new GitHubReleaseClient(httpClient);
        var release = await client.GetLatestReleaseAsync("GyanD", "codexffmpeg", cancellationToken).ConfigureAwait(false);
        return DependencyReleaseResolvers.ResolveFfmpeg(release);
    }

    public async Task InstallAsync(HttpClient httpClient, ResolvedRelease release, CancellationToken cancellationToken)
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var zipPath = Path.Combine(Path.GetTempPath(), $"ffmpeg-{Guid.NewGuid():N}.zip");

        // Extracted under Program Files itself (not %TEMP%) so the final
        // move into InstallDirectory is guaranteed to be same-volume.
        var extractDirectory = Path.Combine(programFiles, $".mediasuite-ffmpeg-tmp-{Guid.NewGuid():N}");

        try
        {
            await Downloader.DownloadToFileAsync(httpClient, release.DownloadUrl, zipPath, cancellationToken)
                .ConfigureAwait(false);

            ZipFile.ExtractToDirectory(zipPath, extractDirectory);

            // The archive contains a single top-level "ffmpeg-<version>-full_build"
            // folder; flatten its contents directly into our install directory.
            var innerFolder = Directory.GetDirectories(extractDirectory).FirstOrDefault()
                ?? throw new InvalidOperationException("The downloaded FFmpeg archive didn't contain the expected folder.");

            if (Directory.Exists(InstallDirectory))
                Directory.Delete(InstallDirectory, recursive: true);

            Directory.Move(innerFolder, InstallDirectory);

            MachinePath.EnsureDirectoryOnPath(Path.Combine(InstallDirectory, "bin"));
        }
        finally
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            if (Directory.Exists(extractDirectory))
                Directory.Delete(extractDirectory, recursive: true);
        }
    }

    public Task RemoveAsync(CancellationToken cancellationToken)
    {
        MachinePath.RemoveDirectoryFromPath(Path.Combine(InstallDirectory, "bin"));

        if (Directory.Exists(InstallDirectory))
            Directory.Delete(InstallDirectory, recursive: true);

        return Task.CompletedTask;
    }
}
