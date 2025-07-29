using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.SemanticKernel.ChatCompletion;
using PrismAI.Core.Models;
using PrismAI.Core.Models.CultureConciergeModels;
using PrismAI.Core.Services;

namespace PrismAI.Client.Services;

public class LlmAgentClientService : IAiAgentService, IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly NavigationManager _navigationManager;
    private bool _isConnected;
    
    public event Action<string>? OnFunctionInvoked;
    public event Action<string>? OnFunctionInvocationCompleted;
    

    public event Action<DemographicsChartDto>? OnDemographicInsightsGenerated;
    

    public LlmAgentClientService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        Task.Run(async () => await EnsureConnectedAsync());
    }
    private TaskCompletionSource<string> _responseTaskCompletionSource = new();
    private TaskCompletionSource<(AudienceAnalysisResult, DemographicsChartDto)> _analysisResultTaskCompletionSource = new();
    private TaskCompletionSource<Experience> _experienceTaskCompletionSource = new();
    private TaskCompletionSource<WebFindResult> _webFindTaskCompletionSource = new();
    private TaskCompletionSource<Recommendation> _alternativeRecommendationTaskCompletionSource = new();
    private TaskCompletionSource<ImageResponse> _imageSearchTaskCompletionSource = new();
    private DemographicsChartDto? _demographicInsightsResponse;
    private event Action<string>? OnChatMessageReceived;
    private async Task EnsureConnectedAsync()
    {
        Console.WriteLine($"\n====================================\nEnsuring connection to SignalR hub...\n====================================\n");
        if (_hubConnection == null)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri("/eventHub"))
                .WithAutomaticReconnect()
                .Build();
            _hubConnection.On<string>("ReceiveLlmAgentResponse", response =>
            {
                Console.WriteLine($"Received response from ReceiveAgentResponse");
                _responseTaskCompletionSource.TrySetResult(response);
            });
            _hubConnection.On<AudienceAnalysisResult, DemographicsChartDto>("ReceiveAnalysisResult", (result, chart) =>
            {
                Console.WriteLine($"Received analysis result from ReceiveAnalysisResult");
                _demographicInsightsResponse = chart;
                _analysisResultTaskCompletionSource.TrySetResult((result,chart));
            });
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
            _hubConnection.On<DemographicsChartDto>("DemographicInsightsGenerated", result =>
            {
                Console.WriteLine($"Demographic insights generated");
                OnDemographicInsightsGenerated?.Invoke(result);
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
            _hubConnection.On<string>("ReceiveChatMessage", message =>
            {
                Console.WriteLine($"Received chat message: {message}");
                OnChatMessageReceived?.Invoke(message);
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
            //var item = _hubConnection.ConnectionId;
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

    public event Action<Experience>? OnExperienceUpdated;

    public async IAsyncEnumerable<string> CultureConceirgeChat(ChatHistory history, Experience experience, UserPreferences preferences,
        string connectionId)
    {
        await EnsureConnectedAsync();
        var stream = _hubConnection!.StreamAsync<string>(
            "CultureConceirgeChat", history, experience, preferences);

        await foreach (var item in stream)
        {
            yield return item;
        }

    }

    public async Task<string> InteractiveAgentChat(ChatHistory history, string creatorBrief)
    {
        await EnsureConnectedAsync();
        // Call the SignalR hub method and return the result (if any)
        // The server method does not return a value, so for now just invoke it
        await _hubConnection!.InvokeAsync("SendLlmAgentQuery", history, creatorBrief);
        // For demo, return a placeholder until server returns a response
        var response = await _responseTaskCompletionSource.Task;
        _responseTaskCompletionSource = new TaskCompletionSource<string>();
        return response;
    }

    public async Task<(AudienceAnalysisResult, DemographicsChartDto?)> GenerateAnalysisResult(CreativeBrief creativeBrief, string connection = "")
    {
        await EnsureConnectedAsync();
        await _hubConnection!.InvokeAsync("GenerateAnalysisResult", creativeBrief, _hubConnection.ConnectionId);
        // For demo, return a placeholder until server returns a response
        var response = await _analysisResultTaskCompletionSource.Task;
        _analysisResultTaskCompletionSource = new TaskCompletionSource<(AudienceAnalysisResult, DemographicsChartDto)>();
        return response;
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