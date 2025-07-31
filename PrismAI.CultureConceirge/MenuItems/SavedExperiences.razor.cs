using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using PrismAI.Core.Models.PrismAIModels;

namespace PrismAI.Components.MenuItems;
public partial class SavedExperiences
{
    [Inject]
    private ILocalStorageService LocalStorageService { get; set; } = default!;
   
    [Parameter]
    public EventCallback<ExperienceRequestResponse> ExperienceSelected { get; set; }
    // List of saved experiences loaded from local storage
    [Parameter]
    public List<ExperienceRequestResponse> SavedExperienceItems { get; set; } = new();
    private const string ExperienceStoragePrefix = "experience_";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load saved experiences when the component is first rendered
            await LoadSavedExperiencesAsync();
        }
    }

    private string? _selectedKey;

    private async Task LoadSelectedExperience(bool prefs = false)
    {
        if (!string.IsNullOrEmpty(_selectedKey))
        {
            var exp = await LocalStorageService.GetItemAsync<ExperienceRequestResponse>(_selectedKey);
            exp.ShowPreferences = prefs;
            await ExperienceSelected.InvokeAsync(exp);
        }
    }

    private async Task DeleteSelectedExperience()
    {
        if (!string.IsNullOrEmpty(_selectedKey))
        {
            await LocalStorageService.RemoveItemAsync(_selectedKey);
            await LoadSavedExperiencesAsync();
            _selectedKey = null;
        }
    }
    
    public async Task LoadSavedExperiencesAsync()
    {
        SavedExperienceItems.Clear();
        var keys = await LocalStorageService.KeysAsync();
        foreach (var key in keys.Where(k => k.Contains(ExperienceStoragePrefix)))
        {
            var exp = await LocalStorageService.GetItemAsync<ExperienceRequestResponse>(key);
            if (exp != null)
                SavedExperienceItems.Add(exp);
        }
        await InvokeAsync(StateHasChanged);
    }
}
