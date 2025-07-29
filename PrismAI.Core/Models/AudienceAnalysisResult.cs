using System.ComponentModel;

namespace PrismAI.Core.Models;

public class AudienceAnalysisResult
{
    [Description("Profile of the target audience, including demographics, interests, platforms, and content preferences.")]
    public AudienceProfile? AudienceProfile { get; set; }
    [Description("List of insights derived from the analysis. Include at least three")]
    public List<Insight> Insights { get; set; } = [];
    [Description("List of actionable suggestions based on the analysis. Include at least three")]
    public List<Suggestion> Suggestions { get; set; } = [];
    
    public string ToMarkdown()
    {
        // Converts the analysis result non-empty string and list properties to a Markdown formatted string.
        var markdown = new System.Text.StringBuilder();
        markdown.AppendLine("# Analysis Result");
        if (AudienceProfile != null)
        {
            markdown.AppendLine("## Audience Profile");
            if (!string.IsNullOrEmpty(AudienceProfile.Demographics))
                markdown.AppendLine($"- **Demographics:** {AudienceProfile.Demographics}");
            if (AudienceProfile.Interests.Count > 0)
                markdown.AppendLine($"- **Interests:** {string.Join(", ", AudienceProfile.Interests)}");
            if (AudienceProfile.Platforms.Count > 0)
                markdown.AppendLine($"- **Platforms:** {string.Join(", ", AudienceProfile.Platforms)}");
            if (AudienceProfile.ContentPreferences.Count > 0)
                markdown.AppendLine($"- **Content Preferences:** {string.Join(", ", AudienceProfile.ContentPreferences)}");
        }
        if (Insights.Count > 0)
        {
            markdown.AppendLine("## Insights");
            foreach (var insight in Insights)
            {
                if (!string.IsNullOrEmpty(insight.Category))
                    markdown.AppendLine($"- **Category:** {insight.Category}");
                if (insight.Confidence > 0)
                    markdown.AppendLine($"- **Confidence:** {insight.Confidence}%");
                if (!string.IsNullOrEmpty(insight.Title))
                    markdown.AppendLine($"- **Title:** {insight.Title}");
                if (!string.IsNullOrEmpty(insight.Description))
                    markdown.AppendLine($"- **Description:** {insight.Description}");
                if (insight.Tags.Count > 0)
                    markdown.AppendLine($"- **Tags:** {string.Join(", ", insight.Tags)}");
            }
        }

        if (Suggestions.Count <= 0) return markdown.ToString();
        markdown.AppendLine("## Suggestions");
        foreach (var suggestion in Suggestions)
        {
            if (!string.IsNullOrEmpty(suggestion.Type))
                markdown.AppendLine($"- **Type:** {suggestion.Type}");
            if (!string.IsNullOrEmpty(suggestion.Title))
                markdown.AppendLine($"- **Title:** {suggestion.Title}");
            if (!string.IsNullOrEmpty(suggestion.Description))
                markdown.AppendLine($"- **Description:** {suggestion.Description}");
            if (!string.IsNullOrEmpty(suggestion.Actionable))
                markdown.AppendLine($"- **Actionable Steps:** {suggestion.Actionable}");
            if (!string.IsNullOrEmpty(suggestion.DataSource))
                markdown.AppendLine($"- **Data Source:** {suggestion.DataSource}");
        }
        return markdown.ToString();

    }


}

public class AudienceProfile
{
    [Description("Demographic information about the audience.")]
    public string? Demographics { get; set; }
    [Description("List of interests associated with the audience.")]
    public List<string> Interests { get; set; } = [];
    [Description("List of platforms where the audience is active.")]
    public List<string> Platforms { get; set; } = [];
    [Description("Content preferences of the audience.")]
    public List<string> ContentPreferences { get; set; } = [];
    [Description("List of Qloo Insights output data item paths and values that support the suggestion.(e.g. `tag.subtype=urn:tag:keyword:media`)")]
    public required List<string> DataItems { get; set; }

}
public class Insight
{
    [Description("Category of the insight.")]
    public string? Category { get; set; }
    [Description("Confidence level of the insight (as a percentage or score).")]
    public int Confidence { get; set; }
    [Description("Title of the insight.")]
    public string? Title { get; set; }
    [Description("Detailed description of the insight.")]
    public string? Description { get; set; }
    [Description("Tags associated with the insight.")]
    public List<string> Tags { get; set; } = [];
    [Description("List of Qloo Insights output data item paths and values that support the suggestion.(e.g. `tag.subtype=urn:tag:keyword:media`)")]
    public required List<string> DataItems { get; set; }
}
public class Suggestion
{
    [Description("Type of suggestion (e.g., strategy, action, improvement).")]
    public string? Type { get; set; }
    [Description("Title of the suggestion.")]
    public string? Title { get; set; }
    [Description("Detailed description of the suggestion.")]
    public string? Description { get; set; }
    [Description("Actionable steps or recommendations.")]
    public string? Actionable { get; set; }
    [Description("Source of the data or reasoning for the suggestion.")]
    public string? DataSource { get; set; }
    [Description("List of Qloo Insights output data item paths and values that support the suggestion.(e.g. `tag.subtype=urn:tag:keyword:media`)")]
    public required List<string> DataItems { get; set; }
}

public class DataItem
{
    [Description("The value of the data item as a string")]
    public string Value { get; set; } = string.Empty;
    [Description("The Qloo Insights output path to support the related content")]
    public string SupportingDataPaths { get; set; } = string.Empty;
}
