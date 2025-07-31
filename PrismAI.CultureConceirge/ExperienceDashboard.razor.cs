using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using PrismAI.Core.Models.Helpers;
using PrismAI.Core.Models.PrismAIModels;
using PrismAI.Core.Models.ResponseModels;
using PrismAI.Core.Services;

namespace PrismAI.Components;

public partial class ExperienceDashboard
{
    [Parameter][EditorRequired] public Experience Experience { get; set; }
    [Parameter] public EventCallback OnStartOver { get; set; }
    [Parameter] public EventCallback ChatAgentOpen { get; set; }
    [CascadingParameter(Name = "Location")]
    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; } = default!;
    [Inject]
    private LocalBrowserStorageService LocalBrowserStorage { get; set; } = default!;
    public string? Location { get; set; }
    private string ActiveCard { get; set; }
    private bool ShowFindMoreModal { get; set; }
    private bool IsLoadingFindMore { get; set; } // Spinner indicator
    [Inject]
    private IAiAgentService AgentService { get; set; } = default!;

    private string _modalText = "";
    private List<RecCard>? _cards;
    private List<RecCard> Cards
    {
        get
        {
            _cards ??= BuildCards();
            return _cards;
        }
        set => _cards = value;
    }

    private ResultsBase Results { get; set; } = new();
    private List<RecCard> BuildCards()
    {
        var cards = new List<RecCard>();
        // Helper to add cards for each recommendation slot
        void AddCard(Recommendation? rec, string key, string icon, string title, int idx)
        {
            if (rec != null && rec.HasSufficientValues())
            {
                var uniqueKey = idx > 0 ? $"{key}-{idx}" : key;
                cards.Add(new RecCard(uniqueKey, icon, title, rec));
            }
        }
        // EntityType-aligned recommendations
        AddCard(Experience?.Artist1, "artist", "🎤", "Artist", 1);
        AddCard(Experience?.Artist2, "artist", "🎤", "Artist", 2);
        AddCard(Experience?.Artist3, "artist", "🎤", "Artist", 3);

        AddCard(Experience?.Book1, "book", "📚", "Book", 1);
        AddCard(Experience?.Book2, "book", "📚", "Book", 2);
        AddCard(Experience?.Book3, "book", "📚", "Book", 3);

        AddCard(Experience?.Brand1, "brand", "🏷️", "Brand", 1);
        AddCard(Experience?.Brand2, "brand", "🏷️", "Brand", 2);
        AddCard(Experience?.Brand3, "brand", "🏷️", "Brand", 3);

        AddCard(Experience?.Destination1, "destination", "🗺️", "Destination", 1);
        AddCard(Experience?.Destination2, "destination", "🗺️", "Destination", 2);
        AddCard(Experience?.Destination3, "destination", "🗺️", "Destination", 3);

        AddCard(Experience?.Movie1, "movie", "🎬", "Movie", 1);
        AddCard(Experience?.Movie2, "movie", "🎬", "Movie", 2);
        AddCard(Experience?.Movie3, "movie", "🎬", "Movie", 3);

        AddCard(Experience?.Person1, "person", "🧑", "Person", 1);
        AddCard(Experience?.Person2, "person", "🧑", "Person", 2);
        AddCard(Experience?.Person3, "person", "🧑", "Person", 3);

        AddCard(Experience?.Place1, "place", "📍", "Place", 1);
        AddCard(Experience?.Place2, "place", "📍", "Place", 2);
        AddCard(Experience?.Place3, "place", "📍", "Place", 3);

        AddCard(Experience?.Podcast1, "podcast", "🎙️", "Podcast", 1);
        AddCard(Experience?.Podcast2, "podcast", "🎙️", "Podcast", 2);
        AddCard(Experience?.Podcast3, "podcast", "🎙️", "Podcast", 3);

        AddCard(Experience?.TvShow1, "tv_show", "📺", "TV Show", 1);
        AddCard(Experience?.TvShow2, "tv_show", "📺", "TV Show", 2);
        AddCard(Experience?.TvShow3, "tv_show", "📺", "TV Show", 3);

        AddCard(Experience?.VideoGame1, "videogame", "🎮", "Video Game", 1);
        AddCard(Experience?.VideoGame2, "videogame", "🎮", "Video Game", 2);
        AddCard(Experience?.VideoGame3, "videogame", "🎮", "Video Game", 3);

        return cards;
    }

    private async Task OnCancel()
    {
        IsLoadingFindMore = false;
        await AgentService.Cancel();
        StateHasChanged();
    }

    private async Task HandleFindRequest(Recommendation recommendation)
    {
        _modalText = $"Finding more about {recommendation.Title}...";
        IsLoadingFindMore = true;
        await InvokeAsync(StateHasChanged);
        Results = await AgentService.GetWebAndVideoRecommendations(recommendation, string.Empty);
        ActiveCard = recommendation.Title;
        ShowFindMoreModal = true;
        IsLoadingFindMore = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleRecChanged(Recommendation recommendation)
    {
        var recTitle = recommendation.Title;
        var populatedRecs = Experience.PopulatedRecommendationSlots();
        var matchedKey = populatedRecs.FirstOrDefault(x => x.Value.Title == recTitle).Key;
        if (!Enum.TryParse<RecommendationSlot>(matchedKey, out var slot)) return;
        Experience.UpdateRecommendation(slot, recommendation);
        await LocalBrowserStorage.UpdateExperience(Experience);
    }
    private async Task HandleGetAlternative(Recommendation recommendation)
    {
        _modalText = $"Finding alternative for {recommendation.Title}...";
        IsLoadingFindMore = true;
        await InvokeAsync(StateHasChanged);
        var location = await LocalBrowserStorage.GetLocationAsync();
        var alternative = await AgentService.GetAlternativeRecommendation(recommendation, string.Empty, location ?? "");
        Console.WriteLine($"Alternative recommendation:\n===============================\n{JsonSerializer.Serialize(alternative, new JsonSerializerOptions() { WriteIndented = true })}");
        // Replace the Card experience with the alternative
        if (alternative != null)
        {
            var card = Cards.FirstOrDefault(c => c.Data.Title == recommendation.Title);
            var cardIndex = Cards.IndexOf(card!);
            var replacement = new RecCard(card!.Key, card.Icon, card.Title, alternative);
            // Replace the existing card with the alternative at the same index
            if (cardIndex >= 0)
                Cards[cardIndex] = replacement;
            else
                Cards.Add(replacement);
        }
        Console.WriteLine($"Cards list after Alternative recommendation:\n===============================\n{JsonSerializer.Serialize(Cards, new JsonSerializerOptions() { WriteIndented = true })}");
        ActiveCard = recommendation.Title;
        IsLoadingFindMore = false;
        await InvokeAsync(StateHasChanged);
    }

    private Location? _location;
    private async Task HandleLocation(Recommendation rec)
    {
        _location = null;
        StateHasChanged();
        await Task.Delay(50);
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"{GetType()}/1.0 (+https://myapp.example.com; contact@myapp.example.com)"
        );
        var url = "https://nominatim.openstreetmap.org/search?q=" + Uri.EscapeDataString(rec.Title) + "&format=json&limit=5";
        Console.WriteLine($"Geocode URL: {url}");
        var responseString = await client.GetStringAsync(url);
        Console.WriteLine($"Geocode: \n\n{responseString}");
        var locations = JsonSerializer.Deserialize<List<LocationGeocoding>>(responseString);
        var lat = double.TryParse(locations.FirstOrDefault().Lat, out var latitude);
        var lon = double.TryParse(locations.FirstOrDefault().Lon, out var longitude);
        var location = new Location() { Lat = latitude, Lon = longitude };
        _location = location;
        StateHasChanged();
    }
    public void LoadExperience()
    {
        Cards = BuildCards();
    }

    private void HideFindMoreModal()
    {
        ShowFindMoreModal = false;
        InvokeAsync(StateHasChanged);
    }
}
public class LocationGeocoding
{
    
    [JsonPropertyName("lat")]
    public string Lat { get; set; }

    [JsonPropertyName("lon")]
    public string Lon { get; set; }
}
public class RecCard(string key, string icon, string title, Recommendation data)
{
    public string Key { get; set; } = key;
    public string Icon { get; set; } = icon;
    public string Title { get; set; } = title;
    public Recommendation Data { get; set; } = data;
}