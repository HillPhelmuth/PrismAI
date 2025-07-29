using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.ResponseModels;

public class AudienceSearchResponse
{
    [JsonPropertyName("totalRequestDuration")]
    public long TotalRequestDuration { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("results")]
    public AudienceResults? Results { get; set; }
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Error>? Errors { get; set; }
    [JsonPropertyName("warnings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Warning>? Warnings { get; set; }
    public List<AudienceSimple> SimpleAudienceData => SimplifyData();

    public List<AudienceSimple> SimplifyData()
    {
        if (Results?.Audiences is null || Results.Audiences.Count == 0) return [];
        return Results.Audiences.Select(a => new AudienceSimple
        {
            Name = a.Name,
            CategoryName = a.Type,
            AudienceEntityId = a.EntityId,
            DisambiguatedName = a.Disambiguation,
            AudienceTagId = a.Id
        }).ToList();
    }
}

public class AudienceSimple
{
    public string? Name { get; set; }
    public string? CategoryName { get; set; }
    public string? AudienceEntityId { get; set; }
    public string? DisambiguatedName { get; set; }
    public string? AudienceTagId { get; set; }
}

public class AudienceResults
{
    [JsonPropertyName("audiences")]
    public List<Audience>? Audiences { get; set; }
}

public class Audience
{
    [JsonPropertyName("entity_id")]
    public string? EntityId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("parents")]
    public List<Parent>? Parents { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; set; }

    [JsonPropertyName("tags")]
    public List<AudienceParent>? Tags { get; set; }
}

public class AudienceParent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}