using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.ResponseModels;

public class InsightsTagsResponseModels
{
}
public class TagResult
{
    [JsonPropertyName("tag_id")]
    public string TagId { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("types")]
    public List<string> Types { get; set; } = null!;   // entity types this tag is associated with

    [JsonPropertyName("subtype")]
    public string Subtype { get; set; } = null!;       // e.g. "urn:tag:keyword:media"

    [JsonPropertyName("tag_value")]
    public string TagValue { get; set; } = null!;      // full URN of the tag (often same as tag_id)

    [JsonPropertyName("query")]
    public object Query { get; set; } = null!;         // (usually an empty object in responses)
}
