namespace PrismAI.Core.Services;

public class QueryRequest(string query)
{
    public string Query { get; set; } = query; // The user query to be processed by the LLM
}