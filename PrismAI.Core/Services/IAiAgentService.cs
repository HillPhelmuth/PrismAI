using Microsoft.SemanticKernel.ChatCompletion;
using PrismAI.Core.Models;
using PrismAI.Core.Models.CultureConciergeModels;

namespace PrismAI.Core.Services;

public interface IAiAgentService
{
    Task<string> InteractiveAgentChat(ChatHistory history, string creatorBrief);
    Task<(AudienceAnalysisResult, DemographicsChartDto?)> GenerateAnalysisResult(CreativeBrief creativeBrief, string connection = "");
    event Action<string>? OnFunctionInvoked;
    event Action<DemographicsChartDto>? OnDemographicInsightsGenerated;
    Task<Experience> GetExperienceRecommendations(UserPreferences preferences, UserProfile userProfile,
        string connectionId = "",
        string locationPoint = "", CancellationToken token = default);
    Task<ResultsBase> GetWebAndVideoRecommendations(Recommendation recommendation, string connectionId,
        CancellationToken token = default);

    Task Cancel();
    Task<Recommendation> GetAlternativeRecommendation(Recommendation recommendation, string connectionId,
        string location = "");
    event Action<string>? OnFunctionInvocationCompleted;
    Task<ImageResponse> RequestImageSearch(Recommendation recommendation, string connectionId = "");
    event Action<Experience>? OnExperienceUpdated;

    IAsyncEnumerable<string> CultureConceirgeChat(ChatHistory history, Experience experience,
        UserPreferences preferences, string connectionId);
}