using System.Net.Http.Json;
using PrismAI.Core.Models;

namespace PrismAI.Core.Services;

public class LlmRequestServiceClient(HttpClient client) : ILlmRequestService
{
    public async Task<InsightsRequest> FormulateRequestAsync(QueryRequest query)
    {
        var response = await client.PostAsJsonAsync("/api/llm/formulate", query);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<InsightsRequest>();
            return result;
        }
        else
        {
            throw new HttpRequestException($"Failed to formulate request: {response.ReasonPhrase}");
        }
    }

    public async Task<string> FunctionCallRequest(QueryRequest query)
    {
        var response = await client.PostAsJsonAsync("/api/llm/function-call", query);
        return await response.Content.ReadAsStringAsync();
    }

    
}