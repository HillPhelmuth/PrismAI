using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QlooHackathonExplore.Core.Models.ResponseModels;

public class EntityTrendsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; }

    [JsonPropertyName("results")]
    public EntityTrendsResults? Results { get; set; }
}

public class EntityTrendsResults
{
    [JsonPropertyName("entities")]
    public List<TrendEntity>? Entities { get; set; }
}

public class TrendEntity
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("entity_id")]
    public string? EntityId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    //[JsonPropertyName("properties")]
    //public Properties Properties { get; set; }

    [JsonPropertyName("popularity")]
    public double Popularity { get; set; }

    [JsonPropertyName("tags")]
    public List<CompareTag>? Tags { get; set; }

    [JsonPropertyName("references")]
    public object? References { get; set; }

    [JsonPropertyName("query")]
    public Query? Query { get; set; }

    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; set; }

    [JsonPropertyName("external")]
    public Dictionary<string, object> External { get; set; }
}



public class ExternalReference
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class Query
{
    [JsonPropertyName("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("rank_delta")]
    public int RankDelta { get; set; }

    [JsonPropertyName("population_percent_delta")]
    public int PopulationPercentDelta { get; set; }

    [JsonPropertyName("population_percentile")]
    public double PopulationPercentile { get; set; }

    [JsonPropertyName("population_percentile_rank")]
    public int PopulationPercentileRank { get; set; }

    [JsonPropertyName("population_percentile_rank_velocity")]
    public int PopulationPercentileRankVelocity { get; set; }

    [JsonPropertyName("trending_score")]
    public int TrendingScore { get; set; }

    [JsonPropertyName("trending_rank")]
    public int TrendingRank { get; set; }

    [JsonPropertyName("charting_score")]
    public int ChartingScore { get; set; }
}

public class CompareTag
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("weight")]
    public double Weight { get; set; }
}