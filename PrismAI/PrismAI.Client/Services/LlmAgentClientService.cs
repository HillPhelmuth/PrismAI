using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.SemanticKernel.ChatCompletion;
using PrismAI.Core.Models.PrismAIModels;
using PrismAI.Core.Services;

namespace PrismAI.Client.Services;

public class LlmAgentClientService : IAiAgentService, IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly NavigationManager _navigationManager;
    private bool _isConnected;
    
    public event Action<string>? OnFunctionInvoked;
    public event Action<string>? OnFunctionInvocationCompleted;
    public event Action<Experience>? OnExperienceUpdated;
    public LlmAgentClientService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        Task.Run(async () => await EnsureConnectedAsync());
    }

    private TaskCompletionSource<Experience> _experienceTaskCompletionSource = new();
    private TaskCompletionSource<WebFindResult> _webFindTaskCompletionSource = new();
    private TaskCompletionSource<Recommendation> _alternativeRecommendationTaskCompletionSource = new();
    private TaskCompletionSource<ImageResponse> _imageSearchTaskCompletionSource = new();

    private async Task EnsureConnectedAsync()
    {
        Console.WriteLine($"\n====================================\nEnsuring connection to SignalR hub...\n====================================\n");
        if (_hubConnection == null)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri("/eventHub"))
                .WithAutomaticReconnect()
                .Build();
           
            _hubConnection.On<string>("FunctionInvoked", functionName =>
            {
                Console.WriteLine($"Function invoked: {functionName}");
                OnFunctionInvoked?.Invoke(functionName);
            });
            _hubConnection.On<string>("FunctionInvocationCompleted", functionName =>
            {
                Console.WriteLine($"Function invocation completed: {functionName}");
                OnFunctionInvocationCompleted?.Invoke(functionName);
            });
            _hubConnection.On<Experience>("ReceiveExperienceRecommendations", experience =>
            {
                Console.WriteLine($"Received experience recommendations");
                _experienceTaskCompletionSource.TrySetResult(experience);
            });
            _hubConnection.On<WebFindResult>("ReceiveWebAndVideoRecommendations", result =>
            {
                Console.WriteLine($"Received web and video recommendations");
                _webFindTaskCompletionSource.TrySetResult(result);
            });
            _hubConnection.On<Recommendation>("ReceiveAlternativeRecommendation", recommendation =>
            {
                Console.WriteLine($"Received alternative recommendation");
                _alternativeRecommendationTaskCompletionSource.TrySetResult(recommendation);
            });
            _hubConnection.On<ImageResponse>("ReceiveImageSearchResults", response =>
            {
                Console.WriteLine($"Received image search response");
                _imageSearchTaskCompletionSource.TrySetResult(response);
            });
            _hubConnection.On<Experience>("ExperienceUpdated", experience =>
            {
                Console.WriteLine($"Experience updated: {experience.Title}");
                OnExperienceUpdated?.Invoke(experience);
            });
        }
        if (_hubConnection.State != HubConnectionState.Connected)
        {
            await _hubConnection.StartAsync();
            _isConnected = true;
        }
    }

    public async Task Cancel()
    {
        await _hubConnection!.InvokeAsync("Cancel");
    }

    public async Task<Recommendation> GetAlternativeRecommendation(Recommendation recommendation, string connectionId,
        string location = "")
    {
        await EnsureConnectedAsync();
        // Call the SignalR hub method to get an alternative recommendation
        await _hubConnection!.InvokeAsync("GetAlternativeRecommendation", recommendation, _hubConnection.ConnectionId, location);
        var response = await _alternativeRecommendationTaskCompletionSource.Task;
        _alternativeRecommendationTaskCompletionSource = new TaskCompletionSource<Recommendation>();
        return response;
    }

    public async Task<ImageResponse> RequestImageSearch(Recommendation recommendation, string connectionId)
    {
        await EnsureConnectedAsync();
        // Call the SignalR hub method to request an image search
        await _hubConnection!.InvokeAsync("RequestImageSearch", recommendation, _hubConnection.ConnectionId);
        var response = await _imageSearchTaskCompletionSource.Task;
        _imageSearchTaskCompletionSource = new TaskCompletionSource<ImageResponse>();
        return response;
    }

    

    public async IAsyncEnumerable<string> PrismAIAgentChat(ChatHistory history, Experience experience, UserPreferences preferences,
        string connectionId)
    {
        await EnsureConnectedAsync();
        var stream = _hubConnection!.StreamAsync<string>(
            "PrismAIAgentChat", history, experience, preferences);

        await foreach (var item in stream)
        {
            yield return item;
        }

    }

    public async Task<Experience> GetExperienceRecommendations(UserPreferences preferences, UserProfile userProfile,
        string connectionId = "",
        string locationPoint = "", CancellationToken token = default)
    {
        await EnsureConnectedAsync();
        // Call the SignalR hub method to get experience recommendations
        await _hubConnection!.InvokeAsync("GetExperienceRecommendations", preferences, userProfile, _hubConnection.ConnectionId, locationPoint, cancellationToken: token);
        var response = await _experienceTaskCompletionSource.Task;
        _experienceTaskCompletionSource = new TaskCompletionSource<Experience>();
        return response;
    }

    public async Task<ResultsBase> GetWebAndVideoRecommendations(Recommendation recommendation, string connectionId,
        CancellationToken token = default)
    {
        await EnsureConnectedAsync();
        await _hubConnection!.InvokeAsync("GetWebAndVideoRecommendations", recommendation, _hubConnection.ConnectionId, cancellationToken: token);
        var response = await _webFindTaskCompletionSource.Task;
        _webFindTaskCompletionSource = new TaskCompletionSource<WebFindResult>();
        return response;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

  
}