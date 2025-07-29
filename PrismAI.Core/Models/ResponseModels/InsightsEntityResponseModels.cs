using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.ResponseModels;

public class InsightsEntityResponseModels
{
}

public class Entity
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; } = null!;   // unique ID (UUID or URN)

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;       // e.g. "urn:entity"

    [JsonPropertyName("subtype")]
    public string Subtype { get; set; } = null!;    // specific type, e.g. "urn:entity:movie", "urn:entity:place"

    [JsonPropertyName("properties")]
    public EntityProperties Properties { get; set; } = null!;  // detailed attributes of the entity

    [JsonPropertyName("disambiguation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Disambiguation { get; set; }   // extra info to distinguish (e.g. year, creators for a movie)

    
    [JsonPropertyName("query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public QueryData? Query { get; set; }  // query data for this entity, e.g. affinity scores, measurements

}
public class QueryData
{
    [JsonPropertyName("affinity")]
    public double Affinity { get; set; }

    [JsonPropertyName("measurements")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Measurements? Measurements { get; set; }
}
public class Measurements
{
    [JsonPropertyName("audience_growth")]
    public double AudienceGrowth { get; set; }
}
// Properties of an Entity (fields vary by entity subtype)
public class EntityProperties
{
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? DurationMinutes { get; set; }

    [JsonPropertyName("image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImageInfo? Image { get; set; }
    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Location? Location { get; set; }  // e.g. lat/lon for places, geohash
    [JsonPropertyName("release_country")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ReleaseCountry { get; set; }
}

// Reusable classes for structured sub-properties:
public class ImageInfo
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;
}
public class Location
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("geohash")]
    public string Geohash { get; set; }
}
