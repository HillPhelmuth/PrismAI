using Markdig;
using Microsoft.AspNetCore.Components;
using PrismAI.Core.Models.PrismAIModels;
using HtmlAgilityPack;

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
        Console.WriteLine($"Markdown before Html Conversion:\n\n{text}\n\n");
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var result = Markdown.ToHtml(text, pipeline);

        // Use HtmlAgilityPack to add target='_blank' to all <a> tags
        var doc = new HtmlDocument();
        doc.LoadHtml(result);
        var links = doc.DocumentNode.SelectNodes("//a");
        if (links == null) return doc.DocumentNode.OuterHtml;
        foreach (var link in links)
        {
            link.SetAttributeValue("target", "_blank");
        }
        return doc.DocumentNode.OuterHtml;
    }
}