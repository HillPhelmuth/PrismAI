using Microsoft.SemanticKernel.ChatCompletion;
using PrismAI.Core.Models.PrismAIModels;

namespace PrismAI.Core.Services;

public interface IAiAgentService
{
    event Action<string>? OnFunctionInvoked;

    Task<Experience> GetExperienceRecommendations(UserPreferences preferences, UserProfile userProfile,
        string connectionId = "", string locationPoint = "", CancellationToken token = default);
    Task<ResultsBase> GetWebAndVideoRecommendations(Recommendation recommendation, string connectionId,
        CancellationToken token = default);

    Task Cancel();
    Task<Recommendation> GetAlternativeRecommendation(Recommendation recommendation, string connectionId,
        string location = "");
    event Action<string>? OnFunctionInvocationCompleted;
    Task<ImageResponse> RequestImageSearch(Recommendation recommendation, string connectionId = "");
    event Action<Experience>? OnExperienceUpdated;

    IAsyncEnumerable<string> PrismAIAgentChat(ChatHistory history, Experience experience,
        UserPreferences preferences, string connectionId);
}