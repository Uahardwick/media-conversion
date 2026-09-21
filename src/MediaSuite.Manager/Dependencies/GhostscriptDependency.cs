using System.Net.Http;
using MediaSuite.Core.Pdf;
using MediaSuite.Core.Processes;
using MediaSuite.Core.Updates;
using MediaSuite.Manager.Shell;

namespace MediaSuite.Manager.Dependencies;

internal sealed class GhostscriptDependency : IManagedDependency
{
    public string Id => "ghostscript";

    public string DisplayName => "Ghostscript";

    public async Task<string?> GetInstalledVersionAsync(CancellationToken cancellationToken)
    {
        var path = GhostscriptLocator.Find();
        if (path is null)
            return null;

        var output = new List<string>();
        await ProcessRunner.RunAsync(path, new[] { "--version" }, onOutputLine: output.Add, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return InstalledVersionParsers.ParseGhostscriptVersion(string.Join(string.Empty, output));
    }

    public async Task<ResolvedRelease> GetLatestReleaseAsync(HttpClient httpClient, CancellationToken cancellationToken)
    {
        var client = new GitHubReleaseClient(httpClient);
        var release = await client.GetLatestReleaseAsync("ArtifexSoftware", "ghostpdl-downloads", cancellationToken).ConfigureAwait(false);
        return DependencyReleaseResolvers.ResolveGhostscript(release);
    }

    public async Task InstallAsync(HttpClient httpClient, ResolvedRelease release, CancellationToken cancellationToken)
    {
        var installerPath = Path.Combine(Path.GetTempPath(), $"gs-installer-{Guid.NewGuid():N}.exe");
        try
        {
            await Downloader.DownloadToFileAsync(httpClient, release.DownloadUrl, installerPath, cancellationToken)
                .ConfigureAwait(false);

            // Ghostscript's Windows installer is NSIS-based; /S is its
            // standard silent-install switch (confirmed against the real
            // installer's embedded manifest, not assumed).
            var result = await ProcessRunner.RunAsync(installerPath, new[] { "/S" }, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (result.ExitCode != 0)
                throw new InvalidOperationException($"Ghostscript installer exited with code {result.ExitCode}.");
        }
        finally
        {
            if (File.Exists(installerPath))
                File.Delete(installerPath);
        }
    }

    public Task RemoveAsync(CancellationToken cancellationToken) =>
        UninstallRegistryHelper.RunUninstallerAsync("Ghostscript", "/S", cancellationToken);
}
