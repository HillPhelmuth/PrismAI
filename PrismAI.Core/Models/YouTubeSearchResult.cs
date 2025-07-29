using System.ComponentModel;
using PrismAI.Core.Models.Helpers;

namespace PrismAI.Core.Models;

public class YouTubeSearchResult : WebSearchResult
{
    public string Id { get; set; }
    public YouTubeThumbnail Thumbnail { get; set; }
    public YouTubeSearchResult() { }

    public YouTubeSearchResult(string id, string title, string description, string videoUrl, YouTubeThumbnail thumbnail)
    {
        Id = id;
        Description = description;
        Url = videoUrl;
        Thumbnail = thumbnail;
        Title = title;
    }
}
public record YouTubeThumbnail(string Url, long? Width, long? Height);
[TypeConverter(typeof(GenericTypeConverter<YouTubeSearchResults>))]
public class YouTubeSearchResults
{
    public List<YouTubeSearchResult> Results { get; set; } = new();
}
