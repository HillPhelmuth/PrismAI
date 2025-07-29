using Google.Apis.Books.v1;
using Google.Apis.Services;
using PrismAI.Core.Models;

namespace PrismAI.Services;

public class GoogleBooksService
{
    private readonly BooksService _booksService;

    public GoogleBooksService(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentNullException(nameof(apiKey));
        _booksService = new BooksService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = GetType().ToString(),
        });
    }

    internal async Task<List<GoogleBooksSearchResult>> ExecuteGoogleBooksApiSearch(string query, int count,
        BookSearchType searchType)
    {
        var searchListRequest = _booksService.Volumes.List(query);
        searchListRequest.MaxResults = count; // Limit to 10 results for simplicity
        searchListRequest.PrintType = searchType == BookSearchType.Book
            ? VolumesResource.ListRequest.PrintTypeEnum.BOOKS
            : VolumesResource.ListRequest.PrintTypeEnum.MAGAZINES;
        var result = await searchListRequest.ExecuteAsync();
        var items = result?.Items;
        return items?.Select(item => new GoogleBooksSearchResult
        {
            Title = item.VolumeInfo?.Title ?? "No Title",
            Authors = string.Join(", ", item.VolumeInfo?.Authors ?? []),
            Description = item.VolumeInfo?.Description ?? "No Description",
            BookThumbnailUrl = item.VolumeInfo?.ImageLinks?.Thumbnail ?? "No Thumbnail",
            Categories = string.Join(", ", item.VolumeInfo?.Categories ?? []),
            AverageRating = item.VolumeInfo?.AverageRating,
            RatingsCount = item.VolumeInfo?.RatingsCount,
            Publisher = item.VolumeInfo?.Publisher ?? "No Publisher",
            PublishedDate = item.VolumeInfo?.PublishedDate ?? "No Published Date",
            Url = item.VolumeInfo?.InfoLink ?? "No Info Link"
        }).ToList() ?? [];
    }

    public async Task<List<GoogleBooksSearchResult>> RunBookSearch(string query, int count)
    {
        return await ExecuteGoogleBooksApiSearch(query, count, BookSearchType.Book);
    }

    public async Task<List<GoogleBooksSearchResult>> RunMagazineSearch(string query, int count)
    {
        return await ExecuteGoogleBooksApiSearch(query, count, BookSearchType.Magazine);
    }
}

internal enum BookSearchType
{
    Book,Magazine
}