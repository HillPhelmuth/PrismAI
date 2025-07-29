using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.ResponseModels;

public class QueryContext
{
    [JsonPropertyName("entities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public QueryEntitiesContext? Entities { get; set; }

    [JsonPropertyName("localities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public QueryLocalitiesContext? Localities { get; set; }
}

public class QueryEntitiesContext
{
    [JsonPropertyName("signal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PartialEntity>? Signal { get; set; }   // entities resolved from signal.interests.entities.query

    [JsonPropertyName("exclude")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PartialEntity>? Exclude { get; set; }  // entities resolved from filter.exclude.entities.query
}

public class QueryLocalitiesContext
{
    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PartialEntity[]? Filter { get; set; }   // locality resolved from filter.location.query

    [JsonPropertyName("signal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PartialEntity? Signal { get; set; }   // locality resolved from signal.location.query
}

// A partial entity representation for query context (contains minimal info about an entity/locality)
public class PartialEntity
{
    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; } = null!;

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("subtype")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Subtype { get; set; }

    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LatLng? Location { get; set; }   // for localities: lat/lng coordinates

    [JsonPropertyName("popularity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Popularity { get; set; }

    [JsonPropertyName("disambiguation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Disambiguation { get; set; }

    [JsonPropertyName("index")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Index { get; set; }   // index of the query item (for multi-query resolutions)
}

public class LatLng
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}