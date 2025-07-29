using PrismAI.Core.Models;

namespace PrismAI.Core.Services;

public interface ILlmRequestService
{
    Task<InsightsRequest> FormulateRequestAsync(QueryRequest query);
    Task<string> FunctionCallRequest(QueryRequest query);
    
}