namespace PrismAI.Core.Models;

public class CreativeBrief
{
    public string ContentType { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public List<string> CulturalReferences { get; set; } = [];
    public string AdditionalContext { get; set; } = string.Empty;
}