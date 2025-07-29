using System.ComponentModel;
using System.Text.Json.Serialization;
using PrismAI.Core.Models.Helpers;

namespace PrismAI.Core.Models.CultureConciergeModels;

[TypeConverter(typeof(GenericTypeConverter<EntityCompareQuery>))]
public class EntityCompareQuery
{
    public required InsightsComparisonQuery ComparisonParameters { get; set; }
}
public class InsightsComparisonQuery
{
    /// <summary>
    /// The first group of entities to base the comparison on.
    /// </summary>
    [Description("The first group of entities to base the comparison on.")]
    [JsonPropertyName("a.signal.interests.entities")]
    public required List<string> FirstEntities { get; set; } = new List<string>();

    /// <summary>
    /// The second group of entities to base the comparison on.
    /// </summary>
    [Description("The second group of entities to base the comparison on.")]
    [JsonPropertyName("b.signal.interests.entities")]
    public required List<string> SecondEntities { get; set; } = new List<string>();

    /// <summary>
    /// The category to search against.
    /// </summary>
    [Description("The category to search against (e.g. urn:entity:movie).")]
    [JsonPropertyName("filter.type")]
    public string? FilterTypes { get; set; }

    /// <summary>
    /// Subtype to filter on (e.g. urn:tag:genre).
    /// </summary>
    [Description("Subtype to filter on (e.g. urn:tag:genre).")]
    [JsonPropertyName("filter.subtype")]
    public string? FilterSubtype { get; set; }
    
   
    /// <summary>
    /// The number of records to return (1–100). Defaults to 20.
    /// </summary>
    [Description("The number of records to return (1–100). Defaults to 20.")]
    [JsonPropertyName("take")]
    public int Take { get; set; } = 30;

    
}