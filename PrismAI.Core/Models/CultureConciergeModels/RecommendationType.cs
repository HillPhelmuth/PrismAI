using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.CultureConciergeModels;

[JsonConverter(typeof(JsonStringEnumConverter<RecommendationType>))]
public enum RecommendationType
{
    Watch,
    Eat,
    Listen,
    Visit,
    Read,
    Buy,
    Play,
    Explore
}