using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PrismAI.Core.Models;

namespace PrismAI.Components.MenuItems;

public partial class ChatInterface
{
    [Parameter] public List<ChatMessage> Messages { get; set; } = [];
    [Parameter] public EventCallback<string> OnSendMessage { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public string? Section { get; set; }
    private string _newMessage = string.Empty;

    private async Task SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(_newMessage))
        {
            await OnSendMessage.InvokeAsync(_newMessage);
            _newMessage = string.Empty;
        }
    }
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e is { Key: "Enter", ShiftKey: false })
        {
            await SendMessage();
        }
    }

    public void UpdateState()
    {
        InvokeAsync(StateHasChanged);
    }
}
public static class ChatMessageExtensions
{
    
    
}