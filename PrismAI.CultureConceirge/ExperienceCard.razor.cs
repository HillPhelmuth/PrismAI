using Microsoft.AspNetCore.Components;
using PrismAI.Core.Models.CultureConciergeModels;
using PrismAI.Core.Services;

namespace PrismAI.Components;

public partial class ExperienceCard
{
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public Recommendation? Recommendation { get; set; }
    [Parameter] public EventCallback<Recommendation> RecommendationChanged { get; set; }
    [Parameter] public bool IsActive { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public EventCallback<Recommendation> RecommendationSelected { get; set; }
    [Parameter] public EventCallback<Recommendation> AlternativeRequested { get; set; }
    [Parameter] public EventCallback<Recommendation> LocationSelected { get; set; }
    private void HandleRecommendationSelect() => RecommendationSelected.InvokeAsync(Recommendation);
    private void HandleAlternativeRequest() => AlternativeRequested.InvokeAsync(Recommendation);
    [Inject]
    private IAiAgentService AgentService { get; set; } = default!;
    

    private bool _isRequesting;
    private string? _imageUrl;
    private bool _showReasoning = false;
    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrEmpty(Recommendation?.ImageUrl) && string.IsNullOrEmpty(_imageUrl) && !_isRequesting)
        {
            await RequestImage();
        }
        else if (string.IsNullOrEmpty(_imageUrl))
        {
            _imageUrl = Recommendation?.ImageUrl;
            StateHasChanged();
        }
        await base.OnParametersSetAsync();
    }

    private async Task RequestImage()
    {
        Console.WriteLine("New Image Requested.");
        _isRequesting = true;
        var response = await AgentService.RequestImageSearch(Recommendation!);
        _imageUrl = response.ImageUrl;
        Console.WriteLine($"New Image url: {_imageUrl}");
        await InvokeAsync(StateHasChanged);
        //await RecommendationChanged.InvokeAsync(Recommendation);

        _isRequesting = false;
        await InvokeAsync(StateHasChanged);
    }

    private void ClickLocation()
    {
        LocationSelected.InvokeAsync(Recommendation);
    }
    private void ToggleReasoning()
    {
        _showReasoning = !_showReasoning;
        IsActive = _showReasoning;
        StateHasChanged();
    }
}