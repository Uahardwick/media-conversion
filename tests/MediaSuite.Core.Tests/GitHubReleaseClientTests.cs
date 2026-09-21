using System.Net;
using MediaSuite.Core.Updates;
using Xunit;

namespace MediaSuite.Core.Tests;

public class GitHubReleaseClientTests
{
    private const string SampleJson = """
        {
          "tag_name": "gs10080",
          "name": "10.08.0",
          "assets": [
            { "name": "gs10080w64.exe", "browser_download_url": "https://example.com/gs10080w64.exe" },
            { "name": "gs10080w32.exe", "browser_download_url": "https://example.com/gs10080w32.exe" }
          ]
        }
        """;

    [Fact]
    public async Task GetLatestReleaseAsync_ParsesTagNameAndAssets()
    {
        var handler = new FakeHandler(SampleJson);
        using var httpClient = new HttpClient(handler);
        var client = new GitHubReleaseClient(httpClient);

        var release = await client.GetLatestReleaseAsync("ArtifexSoftware", "ghostpdl-downloads");

        Assert.Equal("gs10080", release.TagName);
        Assert.Equal("10.08.0", release.Name);
        Assert.Equal(2, release.Assets.Count);
        Assert.Equal("gs10080w64.exe", release.Assets[0].Name);
        Assert.Equal("https://example.com/gs10080w64.exe", release.Assets[0].BrowserDownloadUrl);
    }

    [Fact]
    public async Task GetLatestReleaseAsync_SetsUserAgentHeader()
    {
        var handler = new FakeHandler(SampleJson);
        using var httpClient = new HttpClient(handler);
        var client = new GitHubReleaseClient(httpClient);

        await client.GetLatestReleaseAsync("ArtifexSoftware", "ghostpdl-downloads");

        Assert.NotNull(handler.LastRequest);
        Assert.NotEmpty(handler.LastRequest!.Headers.UserAgent);
        Assert.Equal(
            "https://api.github.com/repos/ArtifexSoftware/ghostpdl-downloads/releases/latest",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetLatestReleaseAsync_Throws_OnNonSuccessStatus()
    {
        var handler = new FakeHandler(SampleJson, HttpStatusCode.NotFound);
        using var httpClient = new HttpClient(handler);
        var client = new GitHubReleaseClient(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetLatestReleaseAsync("owner", "repo"));
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly string _responseBody;
        private readonly HttpStatusCode _statusCode;

        public FakeHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseBody = responseBody;
            _statusCode = statusCode;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody),
            };
            return Task.FromResult(response);
        }
    }
}
