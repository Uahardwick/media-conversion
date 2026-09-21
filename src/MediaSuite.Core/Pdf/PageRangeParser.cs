namespace MediaSuite.Core.Pdf;

/// <summary>
/// Parses a one-based, comma-separated page selection such as "All", "3", or
/// "1, 2-5", deduplicating overlaps and returning pages in ascending order.
/// </summary>
public static class PageRangeParser
{
    public static PageRangeParseResult Parse(string input, int totalPages)
    {
        if (totalPages < 1)
            throw new ArgumentOutOfRangeException(nameof(totalPages), "Document must have at least one page.");

        if (string.IsNullOrWhiteSpace(input))
            return PageRangeParseResult.Fail("Enter a page selection, such as \"All\", \"3\", or \"1, 2-5\".");

        var trimmed = input.Trim();
        if (trimmed.Equals("all", StringComparison.OrdinalIgnoreCase))
            return PageRangeParseResult.Success(Enumerable.Range(1, totalPages).ToList());

        var pages = new SortedSet<int>();
        foreach (var rawToken in trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var token = rawToken.Trim();
            if (token.Length == 0)
                continue;

            var dash = token.IndexOf('-');
            if (dash < 0)
            {
                if (!int.TryParse(token, out var page))
                    return PageRangeParseResult.Fail($"\"{token}\" isn't a valid page number.");

                if (!TryValidatePage(page, totalPages, out var error))
                    return PageRangeParseResult.Fail(error);

                pages.Add(page);
            }
            else
            {
                var startText = token[..dash].Trim();
                var endText = token[(dash + 1)..].Trim();
                if (!int.TryParse(startText, out var start) || !int.TryParse(endText, out var end))
                    return PageRangeParseResult.Fail($"\"{token}\" isn't a valid page range.");

                if (start > end)
                    return PageRangeParseResult.Fail($"\"{token}\" starts after it ends.");

                if (!TryValidatePage(start, totalPages, out var startError))
                    return PageRangeParseResult.Fail(startError);

                if (!TryValidatePage(end, totalPages, out var endError))
                    return PageRangeParseResult.Fail(endError);

                for (var page = start; page <= end; page++)
                    pages.Add(page);
            }
        }

        if (pages.Count == 0)
            return PageRangeParseResult.Fail("Enter a page selection, such as \"All\", \"3\", or \"1, 2-5\".");

        return PageRangeParseResult.Success(pages.ToList());
    }

    private static bool TryValidatePage(int page, int totalPages, out string error)
    {
        if (page < 1)
        {
            error = "Page numbers start at 1.";
            return false;
        }

        if (page > totalPages)
        {
            error = $"This document only has {totalPages} page{(totalPages == 1 ? string.Empty : "s")}.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}

public sealed class PageRangeParseResult
{
    private PageRangeParseResult(bool isValid, string? errorMessage, IReadOnlyList<int> pages)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        Pages = pages;
    }

    public bool IsValid { get; }

    public string? ErrorMessage { get; }

    public IReadOnlyList<int> Pages { get; }

    public static PageRangeParseResult Success(IReadOnlyList<int> pages) => new(true, null, pages);

    public static PageRangeParseResult Fail(string message) => new(false, message, Array.Empty<int>());
}
