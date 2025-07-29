using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.RequestModels;

public class TasteAnalysisRequest
{
    [JsonPropertyName("filter.type")]
    public string Type => "urn:tag";
    public string FilterTypeTags { get; set; }
    [JsonPropertyName("filter.tag.types")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a list of child tag types. You can retrieve a complete list of parent and child tag types via `GetTagTypes`.")]
    public List<string>? TagTypes { get; set; }
    [JsonPropertyName("filter.parents.types")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a list of parental entity types (urn:entity:place). Find parent entity types with `GetTagTypes`.")]
    public List<string>? ParentsTypes { get; set; }
    [JsonPropertyName("signal.interests.entities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("List of valid entity ids (entity ids are in UUID format) used as interest signals")]
    public List<string>? InterestsEntities { get; set; }

    [JsonPropertyName("signal.interests.tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(" tag interests (e.g. `urn:tag:genre:media:horror,urn:tag:genre:media:thriller`)")]
    public List<string>? InterestsTags { get; set; }
    [JsonPropertyName("signal.location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Locality URN or WKT `POINT` representing the location.")]
    public string? Location { get; set; }
    [JsonPropertyName("signal.location.query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("A query used to search for one or more named urn:entity:locality Qloo IDs for filtering requests")]
    public string? LocationQuery { get; set; }
    [JsonPropertyName("take")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The number of records to return. Default is 20, maximum is 50.")]
    public int? Take { get; set; } = 20;
}