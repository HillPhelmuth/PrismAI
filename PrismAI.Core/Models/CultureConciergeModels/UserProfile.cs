using System.ComponentModel;

namespace PrismAI.Core.Models.CultureConciergeModels;

public class UserProfile
{
    public UserAge UserAge { get; set; }
    public string? UserGender { get; set; }
    public string? UserLocation { get; set; }
    public List<string>? UserInterests { get; set; }
    public string? LocationName { get; set; }
    public string UserInterestsString
    {
        get => UserInterests is not null ? string.Join("\n", UserInterests) : "";
        set => UserInterests = value?.Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? [];
    }
}

public enum UserAge
{
    None,
    [Description("35_and_younger")]
    Age35AndYounger,
    [Description("36_to_55")]
    Age36To55,
    [Description("55_and_older")]
    Age55AndOlder
}