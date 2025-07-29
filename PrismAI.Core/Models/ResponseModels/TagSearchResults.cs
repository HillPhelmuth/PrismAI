using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.ResponseModels;

public class TagSearchResult
{
    [JsonPropertyName("totalRequestDuration")]
    public long TotalRequestDuration { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("results")]
    public TagSearchResults Results { get; set; }
}

public class TagSearchResults
{
    [JsonPropertyName("tags")]
    public List<ResultsTag> Tags { get; set; }
}

public class ResultsTag
{
    [JsonPropertyName("parents")]
    public List<Parent> Parents { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("popularity")]
    public double Popularity { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, object> Properties { get; set; }

    [JsonPropertyName("tags")]
    public List<TagTag> Tags { get; set; }
}
public class TagTag
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("tag_id")]
    public string TagId { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}
