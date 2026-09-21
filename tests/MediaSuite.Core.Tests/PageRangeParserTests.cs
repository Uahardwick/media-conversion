using MediaSuite.Core.Pdf;
using Xunit;

namespace MediaSuite.Core.Tests;

public class PageRangeParserTests
{
    [Fact]
    public void All_ReturnsEveryPage()
    {
        var result = PageRangeParser.Parse("All", 5);

        Assert.True(result.IsValid);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.Pages);
    }

    [Fact]
    public void SinglePage_ReturnsThatPage()
    {
        var result = PageRangeParser.Parse("3", 5);

        Assert.True(result.IsValid);
        Assert.Equal(new[] { 3 }, result.Pages);
    }

    [Fact]
    public void Range_ReturnsAllPagesInRange()
    {
        var result = PageRangeParser.Parse("2-5", 5);

        Assert.True(result.IsValid);
        Assert.Equal(new[] { 2, 3, 4, 5 }, result.Pages);
    }

    [Fact]
    public void OverlappingSelections_AreDeduplicatedAndSortedAscending()
    {
        var result = PageRangeParser.Parse("1, 2-5, 4, 1", 5);

        Assert.True(result.IsValid);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.Pages);
    }

    [Fact]
    public void PageBeyondDocument_Fails()
    {
        Assert.False(PageRangeParser.Parse("6", 5).IsValid);
    }

    [Fact]
    public void ZeroPage_Fails()
    {
        Assert.False(PageRangeParser.Parse("0", 5).IsValid);
    }

    [Fact]
    public void ReversedRange_Fails()
    {
        Assert.False(PageRangeParser.Parse("5-2", 5).IsValid);
    }

    [Fact]
    public void EmptyInput_Fails()
    {
        Assert.False(PageRangeParser.Parse("", 5).IsValid);
    }

    [Fact]
    public void GarbageToken_Fails()
    {
        Assert.False(PageRangeParser.Parse("abc", 5).IsValid);
    }
}
