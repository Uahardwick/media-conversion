using System.Net.Http;
using MediaSuite.Core.Updates;

namespace MediaSuite.Manager.Dependencies;

internal interface IManagedDependency
{
    string Id { get; }

    string DisplayName { get; }

    Task<string?> GetInstalledVersionAsync(CancellationToken cancellationToken);

    Task<ResolvedRelease> GetLatestReleaseAsync(HttpClient httpClient, CancellationToken cancellationToken);

    Task InstallAsync(HttpClient httpClient, ResolvedRelease release, CancellationToken cancellationToken);

    Task RemoveAsync(CancellationToken cancellationToken);
}
