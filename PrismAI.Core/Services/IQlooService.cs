using System.Text.Json.Serialization;
using PrismAI.Core.Models;
using PrismAI.Core.Models.CultureConciergeModels;
using PrismAI.Core.Models.RequestModels;
using PrismAI.Core.Models.ResponseModels;

namespace PrismAI.Core.Services;

public interface IQlooService
{
    Task<InsightsResponse> GetInsightsAsync(InsightsRequest request,
        bool requireEntities = false, CancellationToken cancellationToken = default);
    Task<TagsEntityResult> GetAllTagTypesAsync();
    Task<AudiencesResult> GetAllAudienceCategoriesAsync();
    Task<string> SearchForEntities(string query);
    Task<TagSearchResult> GetTagSearchResults(string query);
    Task<AudienceSearchResponse> SearchForAudienceAsync(AudienceSearchRequest request);
    Task<AnalysisResponse> GetAnalysisResponse(AnalysisQueryParameters analysisQuery);
    Task<string> ComareEntities(InsightsComparisonQuery entityCompareQuery);
    Task<string> GetTastesAnalysis(TasteAnalysisRequest tasteAnalysisRequest);
}
public class EntityResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("results")]
    public ResultsItems Results { get; set; }
}

public class ResultsItems
{
    [JsonPropertyName("entities")]
    public object[] Entities { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }
}