using System.Net.Http.Json;
using PrismAI.Core.Models;
using PrismAI.Core.Models.CultureConciergeModels;
using PrismAI.Core.Models.RequestModels;
using PrismAI.Core.Models.ResponseModels;
using PrismAI.Core.Services;

namespace PrismAI.Client.Services;

public class QlooServiceClient(HttpClient client) : IQlooService
{
    public async Task<InsightsResponse> GetInsightsAsync(InsightsRequest request,
        bool requireEntities = false,
        CancellationToken cancellationToken = default)
    {
        var response = await client.PostAsJsonAsync("/api/insights", request, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<InsightsResponse>(cancellationToken: cancellationToken);
        return result;
    }

    public async Task<TagsEntityResult> GetAllTagTypesAsync()
    {
        var response = await client.GetFromJsonAsync<TagsEntityResult>($"/api/tags");
        return response;
    }

    public async Task<AudiencesResult> GetAllAudienceCategoriesAsync()
    {
        var response = await client.GetFromJsonAsync<AudiencesResult>("/api/audiences");
        return response;
    }

    public async Task<string> SearchForEntities(string query)
    {
        var response = await client.GetStringAsync($"/api/entities/{query}");
        return response;
    }

    public async Task<TagSearchResult> GetTagSearchResults(string query)
    {
        var response = await client.GetFromJsonAsync<TagSearchResult>($"/api/tags/search/{query}");
        return response;
    }

    public async Task<AudienceSearchResponse> SearchForAudienceAsync(AudienceSearchRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/audiences/search", request);
        var result = await response.Content.ReadFromJsonAsync<AudienceSearchResponse>();
        return result;
    }
    
    public Task<AnalysisResponse> GetAnalysisResponse(AnalysisQueryParameters analysisQuery)
    {
        throw new NotImplementedException();
    }

    public Task<string> ComareEntities(InsightsComparisonQuery entityCompareQuery)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetTastesAnalysis(TasteAnalysisRequest tasteAnalysisRequest)
    {
        throw new NotImplementedException();
    }
}