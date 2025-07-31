using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using PrismAI.Core.Models.Helpers;
using PrismAI.Core.Models.PrismAIModels;

namespace PrismAI.Components;

public partial class InputForm
{
    [Parameter] public EventCallback<UserPreferences> OnSubmit { get; set; }
    [Parameter]
    public UserPreferences Preferences { get; set; } = new();
    [Inject]
    private ILocalStorageService LocalStorageService { get; set; } = default!;
    private bool _isDebug;
    protected override void OnInitialized()
    {
#if DEBUG
        _isDebug = true;
#endif
        base.OnInitialized();
    }

    private void PopulateTheme(QuickTheme quickTheme)
    {
        Preferences.Theme = $"{quickTheme.CategoryName} - {quickTheme.Description}";
        Preferences.Timeframe = quickTheme.Timeframe;
        InvokeAsync(StateHasChanged);
    }
    //public List<QuickTheme> QuickThemes => QuickThemes;
    public List<QuickThemeCategory> GetQuickThemeCategories()
    {
        return
        [
            new QuickThemeCategory("Cozy Night In",
                QuickThemes.Where(q => q.CategoryName == "Cozy Night In").ToList()),
            new QuickThemeCategory("Date Night", QuickThemes.Where(q => q.CategoryName == "Date Night").ToList()),
            new QuickThemeCategory("Friends & Family Fun",
                QuickThemes.Where(q => q.CategoryName == "Friends & Family Fun").ToList()),
            new QuickThemeCategory("Adventure & Discovery",
                QuickThemes.Where(q => q.CategoryName == "Adventure & Discovery").ToList()),
            new QuickThemeCategory("Themed Escapes",
                QuickThemes.Where(q => q.CategoryName == "Themed Escapes").ToList()),
            new QuickThemeCategory("Mind & Culture",
                QuickThemes.Where(q => q.CategoryName == "Mind & Culture").ToList()),
            new QuickThemeCategory("Relax & Recharge",
                QuickThemes.Where(q => q.CategoryName == "Relax & Recharge").ToList())
        ];
    }

    private static List<QuickTheme> QuickThemes =>
    [
        // Cozy Night In
        new("Cozy Night In", "Classic movie marathon with all the snacks", "🍿", "Evening"),
        new("Cozy Night In", "Dive into a new book with the perfect soundtrack", "📚", "Evening"),
        new("Cozy Night In", "Bake something delicious and binge-watch a comfort show", "🧁", "Evening"),
        new("Cozy Night In", "Explore a new creative hobby (e.g., drawing, writing, coding)", "🎨", "Day"),
        new("Cozy Night In", "Listen to a full album, front-to-back, distraction-free", "🎶", "Evening"),
        new("Cozy Night In", "A nostalgic trip: re-watch a childhood favorite film", "🧸", "Weekend"),
        new("Cozy Night In", "Solve a puzzle or play a solo board game with ambient music", "🧩", "Day"),

        // Date Night
        new("Date Night", "A romantic dinner out and a classic film", "💕", "Evening"),
        new("Date Night", "Competitive and fun: a game night for two", "🎲", "Evening"),
        new("Date Night", "An elegant evening: classical music and fine wine", "🍷", "Evening"),
        new("Date Night", "Laugh-out-loud comedy night with takeout", "😂", "Evening"),
        new("Date Night", "Let's learn something new together (e.g., a dance, a recipe)", "🕺", "Weekend"),
        new("Date Night", "Recreate our first date (at home)", "🏠", "Evening"),
        new("Date Night", "Spontaneous adventure: plan a night based on a random movie genre", "🎬", "Evening"),

        // Friends & Family Fun
        new("Friends & Family Fun", "The ultimate family movie night", "🎥", "Evening"),
        new("Friends & Family Fun", "Host a themed dinner party", "🍽️", "Evening"),
        new("Friends & Family Fun", "Energetic game night with the whole crew", "🕹️", "Evening"),
        new("Friends & Family Fun", "Outdoor cookout with a summer-vibe playlist", "🌭", "Weekend"),
        new("Friends & Family Fun",
            "A collaborative creative project (e.g., making a short film, a group painting)", "🎬", "Weekend"),
        new("Friends & Family Fun", "Potluck-style international food festival at home", "🌎", "Weekend"),
        new("Friends & Family Fun", "Explore our city and nearby cities like tourists for a weekend", "🗺️", "Weekend"),
        new("Friends & Family Fun", "Brunch and a matinee movie marathon", "🥞", "Weekend"),

        // Adventure & Discovery
        new("Adventure & Discovery", "Explore a film genre I've never tried before", "🎞️", "Evening"),
        new("Adventure & Discovery", "Cook a recipe from a country I've never visited", "🍜", "Day"),
        new("Adventure & Discovery", "Discover a new band based on an old favorite", "🎸", "Evening"),
        new("Adventure & Discovery", "A 'blind date' with a book from a new author", "📖", "Day"),
        new("Adventure & Discovery", "Take me down a historical rabbit hole (documentary, book, music)",
            "🏺", "Weekend"),
        new("Adventure & Discovery", "Learn a new practical skill from online tutorials", "💡", "Day"),
        new("Adventure & Discovery", "Plan a micro-adventure in my own neighborhood", "🚶", "Weekend"),

        // Themed Escapes
        new("Themed Escapes", "A cozy whodunit mystery night", "🕵️", "Evening"),
        new("Themed Escapes", "Epic space opera adventure", "🚀", "Evening"),
        new("Themed Escapes", "Transport me to 1920s Paris", "🗼", "Evening"),
        new("Themed Escapes", "A gritty cyberpunk dystopia", "🤖", "Evening"),
        new("Themed Escapes", "Whimsical fantasy realm experience", "🧚", "Evening"),
        new("Themed Escapes", "Retro 80s nostalgia trip", "🕹️", "Evening"),
        new("Themed Escapes", "A sun-drenched Italian coastal holiday", "🏖️", "Weekend"),
        new("Themed Escapes", "An old Hollywood glamour evening", "🎞️", "Evening"),

        // Mind & Culture
        new("Mind & Culture", "An evening with a thought-provoking documentary", "🎬", "Evening"),
        new("Mind & Culture", "Immerse myself in a foreign film and its culture", "🌏", "Evening"),
        new("Mind & Culture", "A literary night: read poetry and listen to instrumental music", "📖", "Evening"),
        new("Mind & Culture", "Explore the life and work of a famous artist", "🖼️", "Day"),
        new("Mind & Culture", "A 'concert hall' experience at home with a legendary live album", "🎤", "Evening"),
        new("Mind & Culture", "Deep-dive into a specific scientific concept", "🔬", "Day"),
        new("Mind & Culture", "Debate night: watch a controversial film and discuss", "🗣️", "Evening"),

        // Relax & Recharge
        new("Relax & Recharge", "At-home spa night with calming music and aromatherapy", "🛁", "Evening"),
        new("Relax & Recharge", "Mindful media: a soothing nature documentary", "🌿", "Evening"),
        new("Relax & Recharge", "Digital detox: a night with books and analog music (e.g., vinyl)",
            "📚", "Evening"),
        new("Relax & Recharge", "Prepare a nourishing, healthy meal from scratch", "🥗", "Day"),
        new("Relax & Recharge", "Gentle stretching or yoga with an ambient soundtrack", "🧘", "Day"),
        new("Relax & Recharge", "A journaling session with inspiring, reflective music", "📓", "Evening"),
        new("Relax & Recharge", "An evening of pure ambient soundscapes for focus or sleep", "🌙", "Evening")
    ];
   

    private List<(string Id, string Label)> Timeframes =
    [
        ("Day", "Today"),
        ("Evening", "This Evening"),
        ("Weekend", "This Weekend"),
        ("Week", "This Week"),

    ];
    
    private void HandleSubmit()
    {
        File.WriteAllText("PreferenceSample.json", JsonSerializer.Serialize(Preferences, new JsonSerializerOptions(){WriteIndented = true}));
        Preferences.EntityTypes = Preferences.EntitySelections.Where(x => x.IsSelected).Select(x => x.EntityType).ToList();
        OnSubmit.InvokeAsync(Preferences);
    }

    private void LoadDemo(int index)
    {
        Preferences = FileHelpers.ExtractFromAssembly<UserPreferences>($"ExamplePref{index}.json");
        InvokeAsync(StateHasChanged);
    }
    //private void OnEntityTypeChanged(ChangeEventArgs e, EntityType entityType)
    //{
    //    if (Preferences.EntityTypes == null)
    //        Preferences.EntityTypes = new List<EntityType>();
    //    var isChecked = e.Value?.ToString() == "true" || e.Value?.ToString() == "on";
    //    if (isChecked)
    //    {
    //        if (!Preferences.EntityTypes.Contains(entityType))
    //            Preferences.EntityTypes.Add(entityType);

    //    }
    //    else
    //    {
    //        Preferences.EntityTypes.Remove(entityType);
    //    }
    //    InvokeAsync(StateHasChanged);
    //}

    // Track expanded categories
    private HashSet<string> ExpandedCategories { get; set; } = new();

    private void ToggleCategory(string categoryName)
    {
        if (!ExpandedCategories.Add(categoryName))
            ExpandedCategories.Remove(categoryName);

        InvokeAsync(StateHasChanged);
    }

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


    private string NewAnchorPreference = string.Empty;

    private async void AddAnchorPreference(string anchor)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(anchor) || Preferences.AnchorPreferences == null ||
                Preferences.AnchorPreferences.Contains(anchor)) return;
            Preferences.AnchorPreferences.Add(anchor);
            //Store preferences in local storage whenever a new anchor is added
            await LocalStorageService.SetItemAsync("UserPreferences", Preferences.AnchorPreferences);
            NewAnchorPreference = string.Empty;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error adding anchor preference: {e.Message}");
        }
    }

    private async void RemoveAnchorPreference(string anchor)
    {
        try
        {
            Preferences.AnchorPreferences?.Remove(anchor);
            //Store preferences in local storage whenever an anchor is removed
            if (Preferences.AnchorPreferences != null)
                await LocalStorageService.SetItemAsync("UserPreferences", Preferences.AnchorPreferences);
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error removing anchor preference: {e.Message}");
        }
    }

    private bool ShowPartnerSection { get; set; } = false;
    private string NewPartnerPreference { get; set; } = string.Empty;

    private void TogglePartnerSection()
    {
        ShowPartnerSection = !ShowPartnerSection;
        InvokeAsync(StateHasChanged);
    }

    private void AddPartnerPreference(string anchor)
    {
        if (string.IsNullOrWhiteSpace(anchor) || Preferences.PartnerPreferences == null ||
            Preferences.PartnerPreferences.Contains(anchor)) return;
        Preferences.PartnerPreferences.Add(anchor);
        NewPartnerPreference = string.Empty;
        InvokeAsync(StateHasChanged);
    }

    private void RemovePartnerPreference(string anchor)
    {
        Preferences.PartnerPreferences?.Remove(anchor);
        InvokeAsync(StateHasChanged);
    }

    private void RemovePartnerSection()
    {
        ShowPartnerSection = false;
        Preferences.PartnerPreferences?.Clear();
        NewPartnerPreference = string.Empty;
        InvokeAsync(StateHasChanged);
    }

    // Track how many anchors to show for each section
    private int VisibleAnchorCount { get; set; } = 8;
    private int VisiblePartnerAnchorCount { get; set; } = 8;

    private void ShowMoreAnchors()
    {
        VisibleAnchorCount = Math.Min(VisibleAnchorCount + 8, PopularAnchorPreferences.Count);
    }

    private void ShowMorePartnerAnchors()
    {
        VisiblePartnerAnchorCount = Math.Min(VisiblePartnerAnchorCount + 8, PopularAnchorPreferences.Count);
    }
}