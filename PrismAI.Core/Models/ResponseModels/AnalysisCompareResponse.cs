using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.ResponseModels;

public class AnalysisCompareResponse
{
    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("results")]
    public AnalysisCompareResult? Results { get; set; }
}

public class AnalysisCompareResult
{
    [JsonPropertyName("tags")]
    public List<CompareTag>? Tags { get; set; }
}

public class CompareTag
{
    [JsonPropertyName("tag_id")]
    public string? TagId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("types")]
    public List<string>? Types { get; set; }

    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    [JsonPropertyName("query")]
    public Query? Query { get; set; }
}

public class Query
{
    [JsonPropertyName("a")]
    public A? A { get; set; }

    [JsonPropertyName("b")]
    public A? B { get; set; }

    [JsonPropertyName("affinity")]
    public double Affinity { get; set; }

    [JsonPropertyName("delta")]
    public double Delta { get; set; }
}

public class A
{
    [JsonPropertyName("affinity")]
    public double Affinity { get; set; }
}