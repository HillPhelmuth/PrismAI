namespace PrismAI.Core.Models.CultureConciergeModels;

public class ExperienceRequestResponse
{
    public string? Key { get; set; }
    public string? DisplayName => Key ?? ResponseExperience?.Title ?? "Saved Experience";
    public UserPreferences RequestPreferences { get; set; } = new();
    public Experience? ResponseExperience { get; set; }
    public bool ShowPreferences { get; set; }
    public DateTime LastUpdate { get; set; } = DateTime.Now;
}