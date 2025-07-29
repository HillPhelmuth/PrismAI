using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.CultureConciergeModels;

public class GeolocationCoordinates
{
    /// <summary>
    /// Position's latitude in decimal degrees.
    /// </summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    /// Position's longitude in decimal degrees.
    /// </summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// Position's altitude in meters, relative to sea level. May be null.
    /// </summary>
    [JsonPropertyName("altitude")]
    public double? Altitude { get; set; }

    /// <summary>
    /// Accuracy of the latitude and longitude, in meters.
    /// </summary>
    [JsonPropertyName("accuracy")]
    public double Accuracy { get; set; }

    /// <summary>
    /// Accuracy of the altitude, in meters. May be null.
    /// </summary>
    [JsonPropertyName("altitudeAccuracy")]
    public double? AltitudeAccuracy { get; set; }

    /// <summary>
    /// Heading in degrees relative to true north. May be null if unavailable or speed is zero.
    /// </summary>
    [JsonPropertyName("heading")]
    public double? Heading { get; set; }

    /// <summary>
    /// Velocity of the device in meters per second. May be null.
    /// </summary>
    [JsonPropertyName("speed")]
    public double? Speed { get; set; }
}
