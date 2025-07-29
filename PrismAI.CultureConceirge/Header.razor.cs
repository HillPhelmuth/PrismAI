using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PrismAI.Core.Models;
using PrismAI.Core.Models.CultureConciergeModels;

namespace PrismAI.Components;

public partial class Header : IAsyncDisposable
{
    [Parameter] public EventCallback OnStartOver { get; set; }
    [Inject]
    private ILocalStorageService LocalStorageService { get; set; } = default!;
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public EventCallback<ExperienceRequestResponse> ExperienceSelected { get; set; }
    // List of saved experiences loaded from local storage
    [Parameter]
    public List<ExperienceRequestResponse> SavedExperienceItems { get; set; } = new();
    
    [Parameter]
    public List<ChatMessage> ChatMessages { get; set; } = new();
    
    [Parameter]
    public EventCallback<string> OnSendMessage { get; set; }
    [Parameter]
    public EventCallback<bool> OnShowChatModal { get; set; }
    [Parameter]
    public EventCallback<bool> OnShowSavedExperiences { get; set; }
    [Parameter]
    public EventCallback<bool> OnShowUserProfile { get; set; }

    [Parameter]
    public bool IsChatLoading { get; set; }
    
    private const string ExperienceStoragePrefix = "experience_";

    private string? _selectedKey;
    private bool _isDropdownOpen = false;
    private bool _showSavedExperiencesModal = false;
    private bool _showChatModal = false;
    private DotNetObjectReference<Header>? _objRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //_objRef = DotNetObjectReference.Create(this);
            //await JSRuntime.InvokeVoidAsync("addClickOutsideHandler", _objRef);
        }
    }

    public void CloseDropdown()
    {
        if (!_isDropdownOpen) return;
        _isDropdownOpen = false;
        InvokeAsync(StateHasChanged);
    }

    private void ToggleDropdown()
    {
        _isDropdownOpen = !_isDropdownOpen;
        StateHasChanged();
    }

    private void OpenSavedExperiencesModal()
    {
        _showSavedExperiencesModal = true;
        OnShowSavedExperiences.InvokeAsync(true);
        _isDropdownOpen = false;
    }

    private void CloseSavedExperiencesModal()
    {
        _showSavedExperiencesModal = false;
        OnShowSavedExperiences.InvokeAsync(false);
    }

    private void OpenChatModal()
    {
        _showChatModal = true;
        OnShowChatModal.InvokeAsync(true);
        _isDropdownOpen = false;
    }

    private void OpenUserProfile()
    {
        OnShowUserProfile.InvokeAsync(true);
    }
    private void CloseChatModal()
    {
        _showChatModal = false;
        OnShowChatModal.InvokeAsync(false);
        _isDropdownOpen = false;
    }

    private async Task HandleExperienceSelected(ExperienceRequestResponse experience)
    {
        await ExperienceSelected.InvokeAsync(experience);
        _showSavedExperiencesModal = false;
    }

    private async Task HandleSendMessage(string message)
    {
        await OnSendMessage.InvokeAsync(message);
    }

    public async ValueTask DisposeAsync()
    {
        if (_objRef != null)
        {
            await JSRuntime.InvokeVoidAsync("removeClickOutsideHandler");
            _objRef.Dispose();
        }
    }
}