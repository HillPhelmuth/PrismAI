using Microsoft.AspNetCore.Components;

namespace PrismAI.Components.MenuItems;
public partial class Menu
{
    [Parameter] public EventCallback OnSavedExperiencesClick { get; set; }
    [Parameter] public EventCallback OnChatClick { get; set; }
    [Parameter] public EventCallback OnStartOver { get; set; }
    [Parameter] public EventCallback OnProfileClick { get; set; }
}
