using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.ResponseModels;

public class AnalysisResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    //[JsonPropertyName("query")]
    //public AnalysisResultQuery Query { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; }

    [JsonPropertyName("results")]
    public AnalysisResults? Results { get; set; }
}

public class AnalysisResults
{
    [JsonPropertyName("tags")]
    public List<AnalysisResultTag>? Tags { get; set; }
}

public class AnalysisResultTag
{
    [JsonPropertyName("tag_id")]
    public string? TagId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    [JsonPropertyName("query")]
    public TagQuery? Query { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class TagQuery
{
    [JsonPropertyName("_tfidf")]
    public double Tfidf { get; set; }

    [JsonPropertyName("_shared_entity_count")]
    public long SharedEntityCount { get; set; }

    [JsonPropertyName("_percentage_of_entities_tagged")]
    public double PercentageOfEntitiesTagged { get; set; }

    [JsonPropertyName("_score")]
    public double Score { get; set; }

    [JsonPropertyName("_percent_of_score")]
    public double PercentOfScore { get; set; }

    [JsonPropertyName("_score_rank")]
    public long ScoreRank { get; set; }

    [JsonPropertyName("_score_percentile")]
    public double ScorePercentile { get; set; }

    [JsonPropertyName("affinity")]
    public double Affinity { get; set; }

    [JsonPropertyName("rank")]
    public long Rank { get; set; }

    [JsonPropertyName("percentile")]
    public double Percentile { get; set; }
}