using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.ResponseModels;

public class InsightsDemographicResponseModels
{
}
public class DemographicResult
{
    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; } = null!;   // the input tag or entity this demographic data pertains to

    [JsonPropertyName("query")]
    public DemographicQuery Query { get; set; } = null!;
}

public class DemographicQuery
{
    [JsonPropertyName("age")]
    public AgeAffinities Age { get; set; } = null!;       // affinity scores for age ranges

    [JsonPropertyName("gender")]
    public GenderAffinities Gender { get; set; } = null!; // affinity scores for genders
}

// Affinity scores by age bracket (note: JSON keys start with numbers, so we map them via JsonPropertyName)
public class AgeAffinities
{
    [JsonPropertyName("24_and_younger")]
    public double? Age24AndYounger { get; set; }

    [JsonPropertyName("25_to_29")]
    public double? Age25To29 { get; set; }

    [JsonPropertyName("30_to_34")]
    public double? Age30To34 { get; set; }

    [JsonPropertyName("35_to_44")]
    public double? Age35To44 { get; set; }

    [JsonPropertyName("45_to_54")]
    public double? Age45To54 { get; set; }

    [JsonPropertyName("55_and_older")]
    public double? Age55AndOlder { get; set; }
}

public class GenderAffinities
{
    [JsonPropertyName("male")]
    public double? Male { get; set; }

    [JsonPropertyName("female")]
    public double? Female { get; set; }
}
