using System.Net.Http.Headers;
using System.Text.Json;

namespace MediaSuite.Core.Updates;

/// <summary>
/// Fetches a public GitHub repository's latest release via the standard
/// REST API. GitHub requires a User-Agent header on every request or it
/// returns 403, so that's always set here.
/// </summary>
public sealed class GitHubReleaseClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public GitHubReleaseClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GitHubRelease> GetLatestReleaseAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{owner}/{repo}/releases/latest");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("MediaToolsSuite", "1.0"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var release = await JsonSerializer.DeserializeAsync<GitHubRelease>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
        return release ?? throw new InvalidOperationException($"GitHub returned an empty release for {owner}/{repo}.");
    }
}
