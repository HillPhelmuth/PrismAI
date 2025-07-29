using System.Text;
using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.ResponseModels;

public class TagsEntityResult
{
    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("results")]
    public TagResults? Results { get; set; }
    public string AsMarkdown => ToMarkdownTable();
    public string ToMarkdown()
    {

        if (Results?.TagTypes == null || Results.TagTypes.Count == 0)
            return string.Empty;

        // Build a flat list of (parent, child) pairs
        var parentChildPairs = Results.TagTypes
            .SelectMany(tt => tt.Parents, (tt, p) => new { Parent = p.Type, Child = tt.Type });

        // Group by parent
        var groups = parentChildPairs
            .GroupBy(pc => pc.Parent)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

        var sb = new StringBuilder();
        foreach (var group in groups)
        {
            sb.AppendLine($"- **{group.Key}**");
            foreach (var child in group
                         .Select(pc => pc.Child)
                         .Distinct()
                         .OrderBy(c => c, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine($"  - {child}");
            }
        }

        return sb.ToString();
    }
    public string ToMarkdownTable()
    {
        if (Results?.TagTypes == null || Results.TagTypes.Count == 0)
            return string.Empty;

        // Flatten into (parent, child) pairs
        var parentChildPairs = Results.TagTypes
            .SelectMany(tt => tt.Parents, (tt, p) => new { Parent = p.Type, Child = tt.Type });

        // Group by parent, sorted
        var groups = parentChildPairs
            .GroupBy(pc => pc.Parent)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

        var sb = new StringBuilder();

        // Header row
        sb.AppendLine("| Parent | Children |");
        sb.AppendLine("| ------ | -------- |");

        foreach (var group in groups)
        {
            // Distinct, sorted children
            var children = group
                .Select(pc => pc.Child)
                .Distinct()
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase);

            // Join with HTML <br/> so line-breaks inside the table cell
            var childrenCell = string.Join("<br/>", children);

            sb.AppendLine($"| {group.Key} | {childrenCell} |");
        }

        return sb.ToString();
    }
}
public class TagResults
{
    [JsonPropertyName("tag_types")] 
    public List<TagType> TagTypes { get; set; } = [];
    public List<ParentChildTagType> ParentChildTagTypes => GetParentChildTagTypes();
    public List<ParentChildTagType> GetParentChildTagTypes()
    {
        var parentChildPairs = TagTypes
            .SelectMany(tt => tt.Parents, (tt, p) => new { Parent = p.Type, Child = tt.Type });

        // Group by parent, sorted
        var groups = parentChildPairs
            .GroupBy(pc => pc.Parent)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);
        return groups
            .Select(g => new ParentChildTagType
            {
                Parent = g.Key,
                Children = g.Select(pc => pc.Child).Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList()
            })
            .ToList();
    }
}

public class ParentChildTagType
{
    [JsonPropertyName("parentTag")]
    public string Parent { get; set; }
    [JsonPropertyName("subTags")]
    public List<string> Children { get; set; }
}

public class TagType
{
    [JsonPropertyName("parents")]
    public List<Parent> Parents { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}
