namespace PrismAI.Core.Models.PrismAIModels;

public class UserPreferences
{
    public string? Theme { get; set; }
    public string? Timeframe { get; set; }
    public List<string>? AnchorPreferences { get; set; } = [];
    public List<string>? PartnerPreferences { get; set; } = [];
    public List<EntityType>? EntityTypes { get; set; } = [];
    private List<EntitySelection>? _entitySelections;
    public List<EntitySelection> EntitySelections
    {
        get
        {
            _entitySelections ??= AvailableEntityTpes.Select(e => new EntitySelection { EntityType = e }).ToList();
            return _entitySelections;
        }
        set => _entitySelections = value;
    }

    public static List<EntityType> AvailableEntityTpes => Enum.GetValues<EntityType>().ToList();
    public string EntityTypeString => 
        string.Join(", ", EntitySelections.Where(x => x.IsSelected).Select(x => x.EntityType.ToString()));
    public string AnchorPreferencesString
    {
        get => AnchorPreferences != null ? string.Join("\n", AnchorPreferences) : "";
        set => AnchorPreferences = value?.Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>();
    }
    public string PartnerPreferencesString
    {
        get => PartnerPreferences != null ? string.Join("\n", PartnerPreferences) : "";
        set => PartnerPreferences = value?.Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>();
    }
    public string? Location { get; set; }

}
public class EntitySelection
{
    public EntityType EntityType { get; set; }
    public bool IsSelected { get; set; }
}
public record QuickTheme(string CategoryName, string Description, string Icon, string Timeframe);

public record QuickThemeCategory(string Name, List<QuickTheme> Themes);
