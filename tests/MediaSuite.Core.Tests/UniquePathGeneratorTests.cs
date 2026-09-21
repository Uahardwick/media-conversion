using MediaSuite.Core.FileSystem;
using Xunit;

namespace MediaSuite.Core.Tests;

public class UniquePathGeneratorTests
{
    [Fact]
    public void ReturnsBaseCandidate_WhenNothingExists()
    {
        var result = UniquePathGenerator.FindAvailable(
            attempt => attempt == 0 ? "Example File" : $"Example File ({attempt + 1})",
            _ => false);

        Assert.Equal("Example File", result);
    }

    [Fact]
    public void SkipsExistingCandidates_InOrder()
    {
        var taken = new HashSet<string> { "Example File", "Example File (2)" };

        var result = UniquePathGenerator.FindAvailable(
            attempt => attempt == 0 ? "Example File" : $"Example File ({attempt + 1})",
            taken.Contains);

        Assert.Equal("Example File (3)", result);
    }

    [Fact]
    public void SupportsFileNamingConventionStartingAtOne()
    {
        var taken = new HashSet<string> { "Beach-YouTube.png" };

        var result = UniquePathGenerator.FindAvailable(
            attempt => attempt == 0 ? "Beach-YouTube.png" : $"Beach ({attempt})-YouTube.png",
            taken.Contains);

        Assert.Equal("Beach (1)-YouTube.png", result);
    }
}
