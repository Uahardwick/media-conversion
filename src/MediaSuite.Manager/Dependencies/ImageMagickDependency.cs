using System.Net.Http;
using MediaSuite.Core.Media;
using MediaSuite.Core.Processes;
using MediaSuite.Core.Updates;
using MediaSuite.Manager.Shell;

namespace MediaSuite.Manager.Dependencies;

internal sealed class ImageMagickDependency : IManagedDependency
{
    public string Id => "imagemagick";

    public string DisplayName => "ImageMagick";

    public async Task<string?> GetInstalledVersionAsync(CancellationToken cancellationToken)
    {
        var path = ImageMagickLocator.Find();
        if (path is null)
            return null;

        var output = new List<string>();
        await ProcessRunner.RunAsync(path, new[] { "-version" }, onOutputLine: output.Add, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return output.Count > 0 ? InstalledVersionParsers.ParseImageMagickVersion(output[0]) : null;
    }

    public async Task<ResolvedRelease> GetLatestReleaseAsync(HttpClient httpClient, CancellationToken cancellationToken)
    {
        var client = new GitHubReleaseClient(httpClient);
        var release = await client.GetLatestReleaseAsync("ImageMagick", "ImageMagick", cancellationToken).ConfigureAwait(false);
        return DependencyReleaseResolvers.ResolveImageMagick(release);
    }

    public async Task InstallAsync(HttpClient httpClient, ResolvedRelease release, CancellationToken cancellationToken)
    {
        var installerPath = Path.Combine(Path.GetTempPath(), $"imagemagick-installer-{Guid.NewGuid():N}.exe");
        try
        {
            await Downloader.DownloadToFileAsync(httpClient, release.DownloadUrl, installerPath, cancellationToken)
                .ConfigureAwait(false);

            // ImageMagick's Windows installer is Inno Setup-based (confirmed
            // via its winget manifest's InstallerType: inno).
            var result = await ProcessRunner.RunAsync(
                installerPath,
                new[] { "/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART", "/SP-" },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (result.ExitCode != 0)
                throw new InvalidOperationException($"ImageMagick installer exited with code {result.ExitCode}.");
        }
        finally
        {
            if (File.Exists(installerPath))
                File.Delete(installerPath);
        }
    }

    public Task RemoveAsync(CancellationToken cancellationToken) =>
        UninstallRegistryHelper.RunUninstallerAsync("ImageMagick", "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART", cancellationToken);
}
