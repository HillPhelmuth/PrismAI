using PrismAI.Core.Models;
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
    Task<string> GetTastesAnalysis(TasteAnalysisRequest tasteAnalysisRequest);
}