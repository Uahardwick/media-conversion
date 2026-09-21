using System.Net.Http;

namespace MediaSuite.Manager.Shell;

internal static class Downloader
{
    public static async Task DownloadToFileAsync(
        HttpClient httpClient,
        string url,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient
            .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var destination = File.Create(destinationPath);
        await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
    }
}
