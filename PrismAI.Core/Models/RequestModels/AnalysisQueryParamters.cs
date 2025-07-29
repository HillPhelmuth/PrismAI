using System.ComponentModel;
using System.Text.Json.Serialization;
using PrismAI.Core.Models.Helpers;

namespace PrismAI.Core.Models.RequestModels;

[TypeConverter(typeof(GenericTypeConverter<AnalysisQuery>))]
public class AnalysisQuery
{
    [Description("Request parameters")]
    public required AnalysisQueryParameters Parameters { get; set; }
}
public class AnalysisQueryParameters
{
    /// <summary>The entities to base the analysis on.</summary>
    [JsonPropertyName("entity_ids")]
    [Description("The entity GUIDs to base the analysis on.")]
    public required List<string> EntityIds { get; set; }

    /// <summary>The category to search against.</summary>
    [JsonPropertyName("filter.type")]
    [Description("The category to search against. Must be one of urn:entity:brand, urn:entity:artist, urn:entity:book, urn:entity:destination, urn:entity:person, urn:entity:place, urn:entity:podcast, urn:entity:movie, urn:entity:tv_show,or urn:entity:videogame")]
    public required string FilterType { get; set; }

    /// <summary>Model to base results on.</summary>
    [JsonPropertyName("model")]
    [Description("Model to base results on. options are 'descriptive' or 'predictive'")]
    public string? Model { get; set; } = "descriptive";

    /// <summary>Subtype to filter on.</summary>
    [JsonPropertyName("filter.subtype")]
    [Description("Subtype to filter on. Must be a valid subtype of `filter.type`")]
    public string? FilterSubtype { get; set; }

    /// <summary>The page number.</summary>
    [JsonPropertyName("page")]
    [Description("The page number.")]
    public int Page { get; set; } = 1;

    /// <summary>The number of records to return.</summary>
    [JsonPropertyName("take")]
    [Description("The number of records to return.")]
    public int Take { get; set; } = 20;

    public string ToQueryString()
    {
        // Builds query string from flattened request object using JsonPropertyName attributes
        var query = new List<string>();
        var type = GetType();
        foreach (var prop in type.GetProperties())
        {
            var value = prop.GetValue(this);
            if (value == null) continue;
            // Skip properties marked with JsonIgnore
            var jsonProp = prop.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                .Cast<JsonPropertyNameAttribute>().FirstOrDefault()?.Name ?? prop.Name.ToLower();
            if (string.IsNullOrEmpty(jsonProp)) continue;
            if (value is IEnumerable<string> stringList)
            {
                var joined = string.Join(",", stringList.Select(Uri.EscapeDataString));
                if (!string.IsNullOrEmpty(joined))
                {
                    query.Add($"{jsonProp}={joined}");
                }
            }
            else
            {
                query.Add($"{jsonProp}={Uri.EscapeDataString(value.ToString()!)}");
            }
        }
        return $"{string.Join("&", query)}";
    }
}