using System.Text.Json.Serialization;
using PrismAI.Core.Models.ResponseModels;

namespace PrismAI.Core.Models;

public class InsightsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("results")]
    public InsightsResults? Results { get; set; }

    [JsonPropertyName("query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public QueryContext? Query { get; set; }

    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Duration { get; set; }   // time taken (ms)
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Error>? Errors { get; set; }
    [JsonPropertyName("warnings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Warning>? Warnings { get; set; }
}
public class InsightsResults
{
    [JsonPropertyName("entities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Entity>? Entities { get; set; }

    [JsonPropertyName("demographics")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<DemographicResult>? Demographics { get; set; }

    [JsonPropertyName("heatmap")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<HeatmapPoint>? Heatmap { get; set; }

    [JsonPropertyName("tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<TagResult>? Tags { get; set; }
    [JsonPropertyName("query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public QueryContext? Query { get; set; }
}
public class Error
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Path { get; set; }
}
public class Warning
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("parameter")]
    public string? Parameter { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}


