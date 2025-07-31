using Microsoft.AspNetCore.Components;
using PrismAI.Core.Models.PrismAIModels;

namespace PrismAI.Components;

public partial class LoadingState
{
    [Parameter] public UserPreferences Preferences { get; set; }

    [Parameter] public List<object> ToolsObjects { get; set; } = [];
    [Parameter] public StepLevel CurrentStep { get; set; } = StepLevel.Analyzing;
    protected override Task OnParametersSetAsync()
    {
        Console.WriteLine($"Tool Calls: {ToolsObjects.Count}");
        Console.WriteLine($"Current Step: {CurrentStep}");
        //if (Steps.Select(x => x.StepLevel).Contains(CurrentStep)) return base.OnParametersSetAsync();

        //var matchStep = AvailableSteps.Find(x => x.StepLevel == CurrentStep);
        //if (matchStep != null)
        //    Steps.Add(matchStep);
        //InvokeAsync(StateHasChanged);
        return base.OnParametersSetAsync();
    }

    public void AddStep(StepLevel stepLevel)
    {
        CurrentStep = stepLevel;
        Steps.ForEach(s => s.IsCurrent = false);
        if (Steps.Select(x => x.StepLevel).Contains(stepLevel)) return;
        var foundStep = AvailableSteps.Find(x => x.StepLevel == stepLevel)!;
        foundStep.IsCurrent = true;
        Steps.Add(foundStep);
        InvokeAsync(StateHasChanged);
    }
    private static readonly List<Step> AvailableSteps =
    [
        new("🔎", "Analyzing your taste profile", 0, StepLevel.Analyzing){IsCurrent = true},
        new("🎨", "Exploring cultural connections", 200, StepLevel.Exploring),
        new("🎵", "Curating cross-domain experiences", 200, StepLevel.Curating),
        new("✨", "Crafting your perfect experience", 200, StepLevel.Crafting)
    ];
    private List<Step> Steps { get; set; } = [AvailableSteps.First(x => x.StepLevel == StepLevel.Analyzing)];

    private record Step(string Icon, string Label, int Delay, StepLevel StepLevel)
    {
        public bool IsCurrent { get; set; }
    }
}
public enum StepLevel
{
    Analyzing,
    Exploring,
    Curating,
    Crafting
}