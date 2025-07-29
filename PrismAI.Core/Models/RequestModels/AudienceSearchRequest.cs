using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.RequestModels;

public class AudienceSearchRequest
{
    [JsonPropertyName("filter.query")]
    [Description("Filter by a simple short query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FilterQuery { get; set; }
    [JsonPropertyName("filter.parents.types")]
    [Description("Filter by a comma-separated list of parental entity types (exact match).")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? FilterParentsTypes { get; set; }

    [JsonPropertyName("filter.results.audiences")]
    [Description("Filter by a comma-separated list of audience IDs.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? FilterResultsAudiences { get; set; }

    [JsonPropertyName("filter.audience.types")]
    [Description("Filter by a list of audience types.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? FilterAudienceTypes { get; set; }

    [JsonPropertyName("filter.popularity.min")]
    [Description("Minimum popularity percentile required (0.0 to 1.0).")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? FilterPopularityMin { get; set; }

    [JsonPropertyName("filter.popularity.max")]
    [Description("Maximum popularity percentile allowed (0.0 to 1.0).")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? FilterPopularityMax { get; set; }

    [JsonPropertyName("page")]
    [Description("Page number of results to return (>=1).")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Page { get; set; }

    [JsonPropertyName("take")]
    [Description("Number of results to return (>=1).")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Take { get; set; }
}

