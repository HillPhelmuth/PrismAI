namespace PrismAI.Core.Models;

public interface ISearchResult
{
    string? Url { get; set; }
    string? Description { get; set; }
    string? Title { get; set; }
}