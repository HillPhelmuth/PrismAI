using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.CultureConciergeModels;

[JsonPolymorphic]
[JsonDerivedType(typeof(YoutubeMusicFindResults), nameof(YoutubeMusicFindResults))]
[JsonDerivedType(typeof(BookFindResults), nameof(BookFindResults))]
[JsonDerivedType(typeof(YoutubeGeneralFindResults), nameof(YoutubeGeneralFindResults))]
[JsonDerivedType(typeof(WebFindResult), nameof(WebFindResult))]
public class ResultsBase
{
    [Description("The summary results with inline citations all in markdown format.")]
    public string? ResultsSummaryWithCitations { get; set; }
}
/// <summary>
/// Represents the result of a web and YouTube search, including summaries and inline citations.
/// </summary>
public class WebFindResult : ResultsBase
{
    /// <summary>
    /// Gets or sets the list of web search results.
    /// </summary>
    [Description("The list of web search results.")]
    public List<WebSearchResult> WebResults { get; set; } = [];

}

public class YoutubeMusicFindResults : ResultsBase
{
    [Description("The list of YouTube **MUSIC** video search results.")]
    public List<YouTubeSearchResult>? YouTubeVideoResults { get; set; }
    
}
public class BookFindResults : ResultsBase
{
    [Description("The list of book search results.")]
    public List<GoogleBooksSearchResult>? BookResults { get; set; }
    
}
public class YoutubeGeneralFindResults: ResultsBase
{
    [Description("The list of YouTube non-music video search results.")]
    public List<YouTubeSearchResult>? YouTubeVideoResults { get; set; }
    
}