using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using PrismAI.Core.Models;
using PrismAI.Core.Models.CultureConciergeModels;
using PrismAI.Core.Services;

namespace PrismAI.Hubs;

public class EventHub : Hub
{
    private readonly IAiAgentService _agentService;

    public EventHub(IAiAgentService agentService)
    {
        _agentService = agentService;
    }
    private CancellationTokenSource _cancellationTokenSource = new();
    public async Task SendLlmAgentQuery(ChatHistory history, string creatorBrief)
    {
        var response = await _agentService.InteractiveAgentChat(history, creatorBrief);
        await Clients.Caller.SendAsync("ReceiveLlmAgentResponse", response);
    }

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
    }
    public async Task GenerateAnalysisResult(CreativeBrief creativeBrief, string connectionId)
    {
        var result = await _agentService.GenerateAnalysisResult(creativeBrief, Context.ConnectionId);
        await Clients.Caller.SendAsync("ReceiveAnalysisResult", result.Item1, result.Item2);
    }
    public async Task GetExperienceRecommendations(UserPreferences preferences, UserProfile userProfile, string connectionId, string locationPoint)
    {
        Console.WriteLine($"GetExperienceRecommendations called with preferences as {preferences.Theme}");
        var token = _cancellationTokenSource.Token;
        var experience = await _agentService.GetExperienceRecommendations(preferences, userProfile, Context.ConnectionId, locationPoint, token);
        await Clients.Caller.SendAsync("ReceiveExperienceRecommendations", experience, cancellationToken: token);
    }
    public async Task GetWebAndVideoRecommendations(Recommendation recommendation, string connectionId)
    {
        Console.WriteLine($"GetWebAndVideoRecommendations called with recommendation as {recommendation.Title}");
        var token = _cancellationTokenSource.Token;
        var result = await _agentService.GetWebAndVideoRecommendations(recommendation, Context.ConnectionId, token);
        await Clients.Caller.SendAsync("ReceiveWebAndVideoRecommendations", result, cancellationToken: token);
    }
    public async Task GetAlternativeRecommendation(Recommendation recommendation, string connectionId, string locationPoint)
    {
        Console.WriteLine($"GetAlternativeRecommendation called with recommendation as {recommendation.Title}");
        var token = _cancellationTokenSource.Token;
        var result = await _agentService.GetAlternativeRecommendation(recommendation, Context.ConnectionId, locationPoint);
        await Clients.Caller.SendAsync("ReceiveAlternativeRecommendation", result, cancellationToken: token);
    }
    public async Task RequestImageSearch(Recommendation recommendation, string connectionId)
    {
        Console.WriteLine($"RequestImageSearch called with recommendation as {recommendation.Title}");
        var token = _cancellationTokenSource.Token;
        var result = await _agentService.RequestImageSearch(recommendation, Context.ConnectionId);
        await Clients.Caller.SendAsync("ReceiveImageSearchResults", result, cancellationToken: token);
    }
    public async IAsyncEnumerable<string> CultureConceirgeChat(ChatHistory history, Experience experience, UserPreferences preferences)
    {
        Console.WriteLine($"CultureConceirgeChat called with history of {history.Count} messages");
        var token = _cancellationTokenSource.Token;
        await foreach (var response in _agentService.CultureConceirgeChat(history, experience, preferences, Context.ConnectionId).WithCancellation(token))
        {
            if (!string.IsNullOrEmpty(response)) yield return response;
            //await Clients.Caller.SendAsync("ReceiveCultureConceirgeResponse", response, cancellationToken: token);
        }
    }
}