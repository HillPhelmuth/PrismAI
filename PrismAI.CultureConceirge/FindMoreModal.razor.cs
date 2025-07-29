using Markdig;
using Microsoft.AspNetCore.Components;
using PrismAI.Core.Models.CultureConciergeModels;

namespace PrismAI.Components;

public partial class FindMoreModal : ComponentBase
{
    [Parameter] public bool Show { get; set; }
    [Parameter] public ResultsBase? Results { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    // Helper properties to get typed results
    private WebFindResult? WebFindResults => Results as WebFindResult;
    private YoutubeGeneralFindResults? YoutubeGeneralResults => Results as YoutubeGeneralFindResults;
    private YoutubeMusicFindResults? YoutubeMusicResults => Results as YoutubeMusicFindResults;
    private BookFindResults? BookResults => Results as BookFindResults;

    private static string AsHtml(string? text)
    {
        if (text == null) return "";
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var result = Markdown.ToHtml(text, pipeline);
        return result;
    }
}