using System.Text.Json.Serialization;

namespace PrismAI.Core.Models;

public class HeatmapResult
{
    [JsonPropertyName("results")]
    public List<HeatmapLocation> Results { get; set; } = [];
}
public class HeatmapLocation
{
    [JsonPropertyName("location")]
    public Location Location { get; set; } = null!;

    [JsonPropertyName("query")]
    public Metrics Query { get; set; } = null!;
}

public class Location   // geographical coordinates
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("geohash")]
    public string Geohash { get; set; } = null!;
}

public class Metrics  // affinity metrics at a location point
{
    [JsonPropertyName("affinity")]
    public double Affinity { get; set; }

    [JsonPropertyName("affinity_rank")]
    public double AffinityRank { get; set; }

    [JsonPropertyName("popularity")]
    public double Popularity { get; set; }
}