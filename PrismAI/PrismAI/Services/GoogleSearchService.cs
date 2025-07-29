using System.Text.Json;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.CustomSearchAPI.v1.Data;
using Google.Apis.Services;
using PrismAI.Plugins;

namespace PrismAI.Services;

public class GoogleSearchService(ILoggerFactory loggerFactory, IConfiguration configuration)
{
    private readonly CustomSearchAPIService _googleCustomSearch = new(new BaseClientService.Initializer
    {
        ApiKey = configuration["Google:ApiKey"],
        ApplicationName = "GoogleSearchService"
    });
    private readonly CrawlService _crawler = new(loggerFactory);
    private readonly ILogger<GoogleSearchService> _logger = loggerFactory.CreateLogger<GoogleSearchService>();

    public async Task<List<SearchResultItem>?> SearchAsync(string query, int answerCount = 10, string? dateRestrict = null)
    {
        var searchRequest = _googleCustomSearch.Cse.List();
        searchRequest.Q = query;
        searchRequest.Cx = configuration["Google:SearchEngineId"];
        searchRequest.Num = answerCount;
        searchRequest.DateRestrict = dateRestrict;
        var resultItem = await searchRequest.ExecuteAsync();
        IList<Result>? pages = resultItem.Items;

        if (pages is null) return [];
        _logger.LogInformation("Search Google Results:\n {result}",
            string.Join("\n", pages.Select(x => x.Link)));
        var searchResultItems = ConvertToSearchResults(pages);
        return searchResultItems;
    }

    public async Task<List<SearchResultItem>> SearchImagesAsync(string query)
    {
        var searchRequest = _googleCustomSearch.Cse.List();
        searchRequest.Q = query;
        searchRequest.Cx = configuration["Google:SearchEngineId"];
        searchRequest.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;
        searchRequest.Num = 10; // Limit to 10 results
        var resultItem = await searchRequest.ExecuteAsync();
        IList<Result>? images = resultItem.Items;
        if (images is null) return [];
        _logger.LogInformation("Search Google Image Results:\n {result}",
            string.Join("\n", images.Select(x => JsonSerializer.Serialize(new {x.Link, x.Image.ThumbnailLink}, new JsonSerializerOptions(){WriteIndented = true}))));
        var searchResultItems = ConvertToSearchResults(images);
        return searchResultItems;
    }
    
    private static List<SearchResultItem> ConvertToSearchResults(IList<Result>? webPages)
    {
        if (webPages is null) return [];
        return webPages.Select(x => new SearchResultItem(x.Link)
        {
            Title = x.Title,
            Content = x.Snippet,
            Url = x.Link,
            Snippet = x.Snippet,
            ImageUrl = x.Link,
        }).ToList();
    }
}
