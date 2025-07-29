using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.ResponseModels;

public class AudiencesResult
{
    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("results")]
    public Results Results { get; set; }

    public string AsMarkdown => ToMarkdown();
    public string ToMarkdown()
    {
        if (Results?.AudienceTypes == null || Results.AudienceTypes.Count == 0)
            return string.Empty;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("### Audience Category Type Tags");
        foreach (var audienceType in Results.AudienceTypes)
        {
            sb.AppendLine($"- **{audienceType.Type}**");
        }
        return sb.ToString();
    }
}

public class Results
{
    [JsonPropertyName("audience_types")]
    public List<AudienceType> AudienceTypes { get; set; }
}

public class AudienceType
{
    [JsonPropertyName("parents")]
    public List<Parent> Parents { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public class Parent
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}



