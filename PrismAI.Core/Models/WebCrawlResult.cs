namespace PrismAI.Core.Models;

public class WebCrawlResult(string url)
{
    public string Url { get; } = url;
    public string? ContentAsMarkdown { get; set; }
    public List<string> LinkUrls { get; set; } = [];
    public string? ContentSummary { get; set; }
}

public class WebSearchResult : ISearchResult
{
    public string? Url { get; set; }
    public string? Description { get; set; }
    public string? Title { get; set; }
    public override string ToString()
    {
        return $"{Title} - {Description} ({Url})";
    }
}
