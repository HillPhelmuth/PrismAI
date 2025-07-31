using Microsoft.AspNetCore.Components;
using PrismAI.Core.Services;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using PrismAI.Core.Models.PrismAIModels;

namespace PrismAI.Components;
public partial class UserProfileForm
{
    [Inject] private LocalBrowserStorageService LocalBrowserStorage { get; set; } = default!;
    [Parameter]
    public EventCallback OnProfileSaved { get; set; }

    private UserProfile _userProfile = new();
    private static readonly List<string> PopularAnchorPreferences =
    [
        // Movies & TV
        "Action movies",
        "Anime TV",
        "Romantic comedies",
        "Live theater",
        "Stand-up comedy",

        // Music
        "Jazz music",
        "Classic rock",
        "Hip-hop music",
        "Electronic dance music (EDM)",
        "Country music",
        "Classical music",
    
        // Games
        "Indie games",
        "Board games",
        "Puzzle games",
        "Escape rooms",

        // Books & Reading
        "Graphic Novels",
        "Historical fiction",
        "Non-fiction bestsellers",
        "Poetry collections",
        "Mystery novels",
        "Science fiction novels",

        // Food & Drink
        "Korean food",
        "Street food",
        "Vegan cuisine",
        "Sushi restaurants",
        "French pastries",
        "Artisanal chocolate",
        "Craft beer",
        "Wine tasting",
        "Coffee culture",

        // Art & Culture
        "Art museums",
        "Photography exhibitions",
        "Documentary podcasts",
        "Nature documentaries",

        // Outdoors & Wellness
        "Hiking trails",
        "Camping",
        "Yoga classes",
        "Beach destinations",
        "National parks",

        // Lifestyle & Hobbies
        "Gardening",
        "Cycling",
        "Home brewing",
        "DIY woodworking",
        "Fantasy sports leagues"
    ];

    private int _visibleInterestCount = 12;
    private string _newInterest = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        // Load user preferences from local storage
        
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var userProfile = await LocalBrowserStorage.GetUserProfileAsync();
            if (userProfile != null)
            {
                _userProfile = userProfile;
                _userProfile.LocationName ??= await ReverseGeoLookup(_userProfile.UserLocation!);
                StateHasChanged();
            }
            else
            {
                var location = await LocalBrowserStorage.GetLocationAsync();
                Console.WriteLine($"Location from local storage: {location}");
                _userProfile = new UserProfile { UserInterests = [], UserLocation = location };
                _userProfile.LocationName ??= await ReverseGeoLookup(_userProfile.UserLocation!);
                StateHasChanged();
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task<string> ReverseGeoLookup(string locationPoint)
    {
        if (string.IsNullOrWhiteSpace(locationPoint))
        {
            Console.WriteLine("Location point is empty or null.");
            return string.Empty;
        }
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"{GetType()}/1.0 (+https://myapp.example.com; contact@myapp.example.com)"
        );
        double lng;
        double lat;
        try
        {
            var (longitude, latitude) = ParsePoint(locationPoint);
            lng = longitude;
            lat = latitude;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error parsing location: {ex.Message}");
            return string.Empty;
        }
        
        var url = $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lng}&format=json";
        Console.WriteLine($"Geocode URL: {url}");
        var responseString = await client.GetStringAsync(url);
        var reverseGeo = JsonSerializer.Deserialize<ReverseGeo>(responseString);
        if (reverseGeo == null)
        {
            Console.WriteLine("Reverse geocode returned null.");
            return string.Empty;
        }
        return $"{reverseGeo.Address.City}, {reverseGeo.Address.State}";
    }
    private class ReverseGeo
    {
        [JsonPropertyName("place_id")]
        public long PlaceId { get; set; }
        
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("address")]
        public Address Address { get; set; }
        
    }

    private class Address
    {

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("county")]
        public string County { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("postcode")]
        public string Postcode { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }
    }
    private static (double Longitude, double Latitude) ParsePoint(string point)
    {
        if (string.IsNullOrWhiteSpace(point))
            throw new ArgumentException("Input cannot be null or empty.", nameof(point));

        const string prefix = "POINT";
        point = point.Trim();

        if (!point.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new FormatException("Input is not in WKT POINT format.");

        var start = point.IndexOf('(');
        var end = point.IndexOf(')');
        if (start < 0 || end < 0 || end <= start)
            throw new FormatException("Input is not in valid WKT POINT format.");

        var content = point.Substring(start + 1, end - start - 1);
        var parts = content
            .Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            throw new FormatException("POINT must contain exactly two coordinate values.");

        var lon = double.Parse(parts[0], CultureInfo.InvariantCulture);
        var lat = double.Parse(parts[1], CultureInfo.InvariantCulture);

        return (lon, lat);
    }
    private void AddInterest(string interest)
    {
        if (string.IsNullOrWhiteSpace(interest)) return;
        _userProfile.UserInterests ??= [];
        if (!_userProfile.UserInterests.Contains(interest))
            _userProfile.UserInterests.Add(interest);
        _newInterest = string.Empty;
    }

    private void RemoveInterest(string interest)
    {
        _userProfile.UserInterests?.Remove(interest);
    }

    private void ShowMoreInterests()
    {
        _visibleInterestCount = Math.Min(PopularAnchorPreferences.Count, _visibleInterestCount + 12);
    }

    private async Task SaveProfile()
    {
        // Save user preferences to local storage
        await LocalBrowserStorage.SaveUserProfileAsync(_userProfile);
        await OnProfileSaved.InvokeAsync();
    }
}
