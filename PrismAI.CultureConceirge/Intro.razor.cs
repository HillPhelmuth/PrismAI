using Microsoft.AspNetCore.Components;

namespace PrismAI.Components;
public partial class Intro : ComponentBase
{
    [Parameter]
    public EventCallback OnStart { get; set; }
    private string cssClass = "intro-container";

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Add a CSS class to the intro container when the component is first rendered
            cssClass += " pop-in";
            InvokeAsync(StateHasChanged);
        }
        return base.OnAfterRenderAsync(firstRender);
    }
}
