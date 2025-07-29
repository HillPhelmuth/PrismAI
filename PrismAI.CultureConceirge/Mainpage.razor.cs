using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PrismAI.Components.MenuItems;
using PrismAI.Core.Models;
using PrismAI.Core.Models.CultureConciergeModels;
using PrismAI.Core.Services;

namespace PrismAI.Components;
public partial class Mainpage
{
    public string CurrentView { get; set; } = "intro";
    public UserPreferences UserPreferences { get; set; } = new();
    public Experience? Experience { get; set; }
    [Inject]
    private IAiAgentService AgentService { get; set; } = default!;
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject]
    private ILocalStorageService LocalStorageService { get; set; } = default!;
    [Inject]
    private LocalBrowserStorageService LocalBrowserStorageService { get; set; } = default!;
    [Inject]
    private PersistentComponentState ApplicationState { get; set; } = default!;
    private PrismAIJsInterop CultureConceirgeJsInterop => new(JsRuntime);

    private List<object> _toolCalls = [];
    private string? _locationPoint;

    private ExperienceDashboard? _experienceDashboard;
    // List of saved experiences loaded from local storage
    public List<ExperienceRequestResponse> SavedExperiences { get; set; } = [];
    private const string ExperienceStoragePrefix = "experience_";

    // Chat functionality
    public List<ChatMessage> ChatMessages { get; set; } = [];
    public bool IsChatLoading { get; set; } = false;
    private ChatInterface? _chatInterface;
    private bool _showUserProfile;
    
    public async Task LoadSavedExperiencesAsync()
    {
        SavedExperiences.Clear();
        SavedExperiences = await LocalBrowserStorageService.LoadExperiencesAsync();
        await InvokeAsync(StateHasChanged);
    }

    // Save the current experience to local storage
    public async Task SaveCurrentExperienceAsync(string? customKey = null)
    {
        
        await LocalBrowserStorageService.SaveCurrentExperienceAsync(Experience, UserPreferences);
        await LoadSavedExperiencesAsync();
    }

    public async Task LoadExperienceAsync(ExperienceRequestResponse exp)
    {
        UserPreferences = exp.RequestPreferences;
        Experience = exp.ResponseExperience;
        CurrentView = exp.ShowPreferences ? "input" : "results";
        await InvokeAsync(StateHasChanged);
        _experienceDashboard?.LoadExperience();
        await CultureConceirgeJsInterop.ScrollToTop();
    }

    // Chat message handling
    public async Task HandleSendMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        // Add user message
        ChatMessages.Add(new ChatMessage
        {
            Role = "user",
            Content = message,
            Timestamp = DateTime.Now
        });

        IsChatLoading = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            var asyncResponse =
                AgentService.CultureConceirgeChat(ChatMessages.ToChatHistory(), Experience, UserPreferences, "");
            await foreach (var item in asyncResponse)
            {
                // Add assistant message token by token
                ChatMessages.UpsertAssistantMessage(item);
                //IsChatLoading = true;
                await InvokeAsync(StateHasChanged);
            }
            
        }
        catch (Exception ex)
        {
            ChatMessages.Add(new ChatMessage
            {
                Role = "assistant",
                Content = $"Sorry, The developer must have really fucked up. Error: {ex.Message}",
                Timestamp = DateTime.Now
            });
        }
        finally
        {
            IsChatLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private int _insightsCalls;
    private void HandleFunctionInvocationCompleted(string functionName)
    {
        if (functionName == "CallQlooEntityInsights") _insightsCalls++;
        Console.WriteLine($"Function invocation completed: {functionName}");
        if (_insightsCalls >= 2)
        {
            _loadingState?.AddStep(StepLevel.Crafting);
        }
        InvokeAsync(StateHasChanged);
    }
    private bool _showSavedExperiencesModal = false;
    private bool _showChatModal = false;
    private void HandleShowChat(bool show)
    {
        _showChatModal = show;
        InvokeAsync(StateHasChanged);
    }
    private void HandleShowSavedExperiences(bool show)
    {
        _showSavedExperiencesModal = show;
        InvokeAsync(StateHasChanged);
    }
    private StepLevel _currentStep = StepLevel.Analyzing;
    private LoadingState? _loadingState;
    private void HandleFunctionInvoked(string json)
    {
        
        var asFunctionCall = JsonSerializer.Deserialize<FunctionCall>(json);

        switch (asFunctionCall?.Name)
        {
            case "SearchForTags" or "SearchForEntities" when _currentStep == StepLevel.Analyzing:
                _loadingState?.AddStep(StepLevel.Exploring);
                _currentStep = StepLevel.Exploring;
                InvokeAsync(StateHasChanged);
                break;
            case "CallQlooEntityInsights" when _currentStep == StepLevel.Exploring:
                _loadingState?.AddStep(StepLevel.Curating);
                _currentStep = StepLevel.Curating;
                InvokeAsync(StateHasChanged);
                break;
        }
        var obj = JsonSerializer.Deserialize<object>(json);
        _toolCalls.Add(obj);
        if (_toolCalls.Count > 5)
            _toolCalls.RemoveAt(0);
        InvokeAsync(StateHasChanged);
    }

    private Header _header;
    private void HandleOuterClick()
    {
        _header.CloseDropdown();
    }
    private class FunctionCall
    {
        public string? Name { get; init; }
        public Dictionary<string, object> Arguments { get; init; } = [];
    }
    public async Task HandlePreferencesSubmit(UserPreferences preferences)
    {
        UserPreferences = preferences;
        var profile = await LocalBrowserStorageService.GetUserProfileAsync();
        CurrentView = "loading";
        await InvokeAsync(StateHasChanged);
        await Task.Delay(1);
        Experience = await AgentService.GetExperienceRecommendations(preferences, profile, locationPoint: _locationPoint ?? "");
        File.WriteAllText("Experience.json", JsonSerializer.Serialize(Experience, new JsonSerializerOptions() { WriteIndented = true }));
        CurrentView = "results";
        await InvokeAsync(StateHasChanged);
        await CultureConceirgeJsInterop.ScrollToTop();
        await SaveCurrentExperienceAsync(); // Save after generating new experience
    }

    public void HandleStartOver()
    {
        CurrentView = "intro";
        UserPreferences = new UserPreferences();
        Experience = null;
        _toolCalls = [];
        _currentStep = StepLevel.Analyzing;
        InvokeAsync(StateHasChanged);

    }

    private async void HandleProfileSaved()
    {
        try
        {
            CurrentView = "input";
            _showUserProfile = false;
            await InvokeAsync(StateHasChanged);
            await CultureConceirgeJsInterop.ScrollToTop();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in HandleProfileSaved: {e.Message}");
        }
    }
    private async void HandleProfileEdit()
    {
        try
        {
            _showUserProfile = true;
            await CultureConceirgeJsInterop.ScrollToTop();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in HandleProfileEdit: {e.Message}");
        }
    }
    public async void HandleIntroStart()
    {
        try
        {
            var profile = await LocalBrowserStorageService.GetUserProfileAsync();
            CurrentView = profile is null ? "profile" : "input";
            await InvokeAsync(StateHasChanged);
            await CultureConceirgeJsInterop.ScrollToTop();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in HandleIntroStart: {e.Message}");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            AgentService.OnFunctionInvoked += HandleFunctionInvoked;
            AgentService.OnFunctionInvocationCompleted += HandleFunctionInvocationCompleted;
            AgentService.OnExperienceUpdated += HandleExperienceUpdated;
            var savedLocation = await LocalBrowserStorageService.GetLocationAsync();
            if (!string.IsNullOrWhiteSpace(savedLocation))
            {
                _locationPoint = savedLocation;
                Console.WriteLine($"Loaded saved location point: {_locationPoint}");
            }
            else
            {
                Console.WriteLine("No saved location found, fetching current location...");
                var location = await CultureConceirgeJsInterop.GetCurrentLocation();

                if (location != null)
                {
                    _locationPoint = $"POINT ({location.Longitude} {location.Latitude})";
                    await LocalBrowserStorageService.SaveLocation(_locationPoint);
                    Console.WriteLine($"Saved current location point: {_locationPoint}");
                }
                else
                {
                    Console.WriteLine("Failed to retrieve current location.");
                }
                Console.WriteLine($"Location Point: {_locationPoint}");
            }
            UserPreferences.Location = _locationPoint;
            await LoadSavedExperiencesAsync(); // Load saved experiences on first render
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async void HandleExperienceUpdated(Experience experience)
    {
        
        try
        {
            Console.WriteLine("Experience updated in Mainpage component");
            Experience = experience;
        
            await InvokeAsync(StateHasChanged);
            _experienceDashboard?.LoadExperience();
            await SaveCurrentExperienceAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in HandleExperienceUpdated: {e.Message}");
        }
    }
}
