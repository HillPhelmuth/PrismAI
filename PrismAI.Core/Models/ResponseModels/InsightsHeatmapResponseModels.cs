using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.ResponseModels;

public class InsightsHeatmapResponseModels
{
}
public class HeatmapPoint
{
    [JsonPropertyName("location")]
    public GeoLocation Location { get; set; } = null!;

    [JsonPropertyName("query")]
    public HeatmapMetrics Query { get; set; } = null!;
}

public class GeoLocation   // geographical coordinates
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("geohash")]
    public string Geohash { get; set; } = null!;
}

public class HeatmapMetrics  // affinity metrics at a location point
{
    [JsonPropertyName("affinity")]
    public double Affinity { get; set; }

    [JsonPropertyName("affinity_rank")]
    public double AffinityRank { get; set; }

    [JsonPropertyName("popularity")]
    public double Popularity { get; set; }
}
