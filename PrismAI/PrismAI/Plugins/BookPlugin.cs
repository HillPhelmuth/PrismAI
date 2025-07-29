using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using PrismAI.Services;

namespace PrismAI.Plugins;

public class BookPlugin
{
    private readonly GoogleBooksService _googleBooksService;
    public BookPlugin(string googleBooksApiKey)
    {
        if (string.IsNullOrEmpty(googleBooksApiKey))
            throw new ArgumentNullException(nameof(googleBooksApiKey));
        _googleBooksService = new GoogleBooksService(googleBooksApiKey);
    }

    [KernelFunction, Description("Search Google Books for books. Outputs a json array of book data")]
    public async Task<string> SearchBooks(
        [Description("Google Books search query")] string query,
        [Description("Number of results to return")] int count = 10)
    {
        if (string.IsNullOrEmpty(query))
            throw new ArgumentNullException(nameof(query));
        var results = await _googleBooksService.RunBookSearch(query, count);
        return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
    }
}