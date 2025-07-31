using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PrismAI.Core.Models;
using PrismAI.Core.Models.Helpers;
using PrismAI.Core.Models.PrismAIModels;
using PrismAI.Core.Services;
using PrismAI.Hubs;
using PrismAI.Plugins;
using PrismAI.Services.HttpHandlers;

namespace PrismAI.Services;

public class LlmRequestService : IAiAgentService
{
    private readonly AutoInvokeFilter _autoInvokeFilter = new();
    private readonly IConfiguration _configuration;
    private readonly IQlooService _qlooService;
    private readonly IHubContext<EventHub> _hubContext;
    private readonly ILoggerFactory _loggerFactory;
    public LlmRequestService(IConfiguration configuration, IQlooService qlooService, IHubContext<EventHub> hubContext, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _qlooService = qlooService;
        _hubContext = hubContext;
        _loggerFactory = loggerFactory;
        _autoInvokeFilter.AutoFunctionInvocationStarted += OnAutoFunctionInvocationStarted;
        _autoInvokeFilter.AutoFunctionInvocationCompleted += OnAutoFunctionInvocationCompleted;
    }

    private void OnAutoFunctionInvocationCompleted(AutoFunctionInvocationContext context, string connectionId)
    {
        var functionName = context.Function.Name;
        if (!string.IsNullOrEmpty(connectionId))
            _hubContext.Clients.Client(connectionId).SendAsync("FunctionInvocationCompleted", functionName);
        else
            OnFunctionInvocationCompleted?.Invoke(functionName);
        if (functionName is "UpdateUserExperience" or "RemoveUserRecommendation")
        {
            var json = context.Result.ToString();
            var success = JsonHelpers.TryDeserializeJson<Experience>(json, out var experience);
            if (!string.IsNullOrEmpty(connectionId))
                _hubContext.Clients.Client(connectionId).SendAsync("ExperienceUpdated", experience);
            else
            {
                if (success)
                    OnExperienceUpdated?.Invoke(experience!);
                else
                    Console.WriteLine($"Failed to Convert\n{json}\nTo Experience object");
            }
        }
    }

    private CancellationTokenSource _cancellationTokenSource = new();
    public event Action<string>? OnFunctionInvoked;
    public event Action<string>? OnFunctionInvocationCompleted;
    public event Action<Experience>? OnExperienceUpdated;

    private void OnAutoFunctionInvocationStarted(AutoFunctionInvocationContext context, string connectionId)
    {
        var functionObj = new { context.Function.Name, context.Arguments };
        var json = JsonSerializer.Serialize(functionObj);
        if (!string.IsNullOrEmpty(connectionId))
            _hubContext.Clients.Client(connectionId).SendAsync("FunctionInvoked", json);
        else
        {
            OnFunctionInvoked?.Invoke(json);
        }
    }

    public async Task<Experience> GetExperienceRecommendations(UserPreferences preferences, UserProfile userProfile,
        string connectionId, string locationPoint = "", CancellationToken token = default)
    {
        //var agent = CreateExperienceAgent();
        var kernel = CreateKernel(connectionId, location: locationPoint);
        kernel.Data["age"] = userProfile.UserAge;
        kernel.Data["gender"] = userProfile.UserGender is "male" or "female" ? userProfile.UserGender : null;
        var qloo = kernel.ImportPluginFromType<QlooPlugin>();
        Console.WriteLine($"Qloo plugin functions:\n=================================\n{string.Join("\n", qloo.Select(x => x.Name))}");
        var googleSearch = new GoogleSearchService(_loggerFactory, _configuration);
        var webCrawlPlugin = new WebCrawlPlugin(googleSearch, _loggerFactory, _configuration);
        var webPlugin = kernel.ImportPluginFromObject(webCrawlPlugin);
        var youtubeService = new YouTubePlugin(_configuration["YouTube:ApiKey"]!);
        var youtubePlugin = kernel.ImportPluginFromObject(youtubeService);

        
        List<string> unneededFunctions = ["GenerateHeatmap", "GenerateLocationsMap", "CallQlooTastes",
            "CallQlooDemographicInsights"];
        var options = new FunctionChoiceBehaviorOptions()
        { AllowConcurrentInvocation = true, AllowParallelCalls = false };
        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(functions: qloo.Where(x => !unneededFunctions.Any(y => x.Name.Contains(y))).Concat(webPlugin.Where(webFunc => webFunc.Name.Contains("SearchWebImages"))), options: options), ResponseFormat = typeof(Experience) };
        Console.WriteLine($"OpenAI Exec Settings:\n============================\n{JsonSerializer.Serialize(settings.Clone(), new JsonSerializerOptions() { WriteIndented = true })}\n============================\n");
        var allowedEntities = await GetEntityTypeResponse(preferences, connectionId, token);
        kernel.Data["allowedEntities"] = allowedEntities.EntityTypeResponseItems.Select(x => x.EntityType).ToList();
        var allowedEntityCountBuilder = new StringBuilder();
        foreach (var group in allowedEntities.EntityTypeResponseItems)
        {
            allowedEntityCountBuilder.AppendLine($"Number of {group.EntityType} recommendations - {group.NumberOfRecommendationSlots}");
        }
        Console.WriteLine($"Allowed Entity String: {allowedEntityCountBuilder.ToString()}");
        var args = new KernelArguments(settings)
        {
            ["theme"] = preferences.Theme ?? "none",
            ["timeframe"] = preferences.Timeframe ?? "none",
            ["anchorPreferences"] = userProfile.UserInterestsString,
            ["preferredEntities"] = allowedEntityCountBuilder.ToString(),
            ["userLocation"] = locationPoint,
            ["partnerAnchorPreferences"] = string.IsNullOrEmpty(preferences.PartnerPreferencesString) ? "no partner" : preferences.PartnerPreferencesString

        };
        
        var response = await kernel.InvokePromptAsync<string>(Prompts.PrismAIExperienceCuratorPrompt, args, cancellationToken: token);
        Console.WriteLine($"\n==========================\nRaw LLM Response from GetExperienceRecommendations\n==========================\n{response}");

        return JsonSerializer.Deserialize<Experience>(response);
    }
    private async Task<EntityTypeResponses> GetEntityTypeResponse(UserPreferences preferences, string connectionId, CancellationToken token = default)
    {
        var kernel = CreateKernel(connectionId, "gpt-4.1-mini");
        var settings = new OpenAIPromptExecutionSettings() { ResponseFormat = typeof(EntityTypeResponses) };
        var args = new KernelArguments(settings)
        {
            ["theme"] = preferences.Theme ?? "none",
            ["timeframe"] = preferences.Timeframe ?? "none",
            ["requestedEntities"] = preferences.EntityTypeString
        };
        var response =
            await kernel.InvokePromptAsync<string>(Prompts.EntityCategoryHelperPrompt, args, cancellationToken: token);
        Console.WriteLine($"Recommended Entity Categories:\n=============================\n{response}\n=============================\n");
        var entityTypeResponses = JsonSerializer.Deserialize<EntityTypeResponses>(response);
        return entityTypeResponses!;
    }

   
    public async Task<ResultsBase> GetWebAndVideoRecommendations(Recommendation recommendation, string connectionId,
        CancellationToken token = default)
    {
        var routerResponse = await RouteRequestAsync(recommendation);
        Console.WriteLine($"Router response: {routerResponse.AgentType}");
        return routerResponse.AgentType switch
        {
            AgentType.WebRecommender => await GetWebFindResult(recommendation, connectionId),
            AgentType.BookRecommender => await GetBookFindResults(recommendation, connectionId),
            AgentType.MusicRecommender => await GetYoutubeMusicResult(recommendation, connectionId),
            AgentType.YoutubeSearch => await GetYoutubeSearchResult(recommendation, connectionId),
            _ => await GetWebFindResult(recommendation, connectionId)
        };
    }

    public async Task<Recommendation> GetAlternativeRecommendation(Recommendation recommendation, string connectionId,
        string location = "")
    {
        Console.WriteLine($"GetAlternativeRecommendation called with recommendation: {recommendation.Title}");
        var kernel = CreateKernel(connectionId);
        var qloo = kernel.ImportPluginFromType<QlooPlugin>();
        Console.WriteLine($"Qloo plugin functions:\n=================================\n{string.Join("\n", qloo.Select(x => x.Name))}");
        List<string> unneededFunctions = ["GenerateHeatmap", "GenerateLocationsMap", "CallQlooTastes",
            "CallQlooDemographicInsights" ];
        var isLocation = recommendation.EntityTypeId.Contains("place") || recommendation.EntityTypeId.Contains("destination");
        unneededFunctions.Add(isLocation ? "FindRelatedAlternativeEntities" : "CallQlooEntityInsights");

        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(functions: qloo.Where(x => !unneededFunctions.Any(y => x.Name.Contains(y)))), ResponseFormat = typeof(Recommendation) };
        var args = new KernelArguments(settings) { ["recommendation"] = JsonSerializer.Serialize(recommendation, new JsonSerializerOptions { WriteIndented = true }), ["userLocation"] = location };
        var alternativeRecommendationPrompt = isLocation ? Prompts.AlternativeLocationRecommendationPrompt : Prompts.AlternativeRecommendationPrompt;
        var response = await kernel.InvokePromptAsync<string>(alternativeRecommendationPrompt, args);
        Console.WriteLine($"\n==========================\nRaw LLM Response from GetAlternativeRecommendation\n==========================\n{response}");
        return JsonSerializer.Deserialize<Recommendation>(response)!;
    }

    public async Task<ImageResponse> RequestImageSearch(Recommendation recommendation, string connectionId)
    {

        var kernel = CreateKernel(connectionId, "gpt-4.1-mini");
        var googleSearch = new GoogleSearchService(_loggerFactory, _configuration);
        var webCrawlPlugin = new WebCrawlPlugin(googleSearch, _loggerFactory, _configuration);
        var webPlugin = kernel.ImportPluginFromObject(webCrawlPlugin);
        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(functions: webPlugin.Where(webFunc => webFunc.Name.Contains("SearchWebImages"))), ResponseFormat = typeof(ImageResponse) };
        var args = new KernelArguments(settings)
        {
            ["recommendation"] =
                JsonSerializer.Serialize(recommendation, new JsonSerializerOptions { WriteIndented = true }),
            ["currentImageUrl"] = recommendation.ImageUrl
        };
        var response = await kernel.InvokePromptAsync<string>(Prompts.ImageSearchAgentPrompt, args);
        Console.WriteLine($"\n==========================\nRaw LLM Response from RequestImageSearch\n==========================\n{response}");
        return JsonSerializer.Deserialize<ImageResponse>(response)!;
    }

    public async IAsyncEnumerable<string> PrismAIAgentChat(ChatHistory history, Experience experience,
        UserPreferences preferences, string connectionId)
    {
        var kernel = CreateKernel(connectionId, location: preferences.Location);
        var currentExperienceJson = JsonSerializer.Serialize(experience);
        kernel.Data["currentExperienceJson"] = currentExperienceJson;
        var locationPoint = preferences.Location ?? "none";
        kernel.Data["location"] = locationPoint;
        var qloo = kernel.ImportPluginFromType<QlooPlugin>();
        Console.WriteLine($"Qloo plugin functions:\n=================================\n{string.Join("\n", qloo.Select(x => x.Name))}");
        var googleSearch = new GoogleSearchService(_loggerFactory, _configuration);
        var webCrawlPlugin = new WebCrawlPlugin(googleSearch, _loggerFactory, _configuration);
        var webPlugin = kernel.ImportPluginFromObject(webCrawlPlugin);
        var youtubeService = new YouTubePlugin(_configuration["YouTube:ApiKey"]!);
        var youtubePlugin = kernel.ImportPluginFromObject(youtubeService);
        var expUpdatePlugin = kernel.ImportPluginFromType<ExperienceUpdatePlugin>();
        
        List<string> unneededFunctions = ["GenerateHeatmap", "GenerateLocationsMap", "CallQlooTastes",
            "CallQlooDemographicInsights","SearchForEntities"];
        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(functions: qloo.Where(x => !unneededFunctions.Any(y => x.Name.Contains(y))).Concat(webPlugin).Concat(youtubePlugin).Concat(expUpdatePlugin)) };

        var args = new KernelArguments()
        {
            ["theme"] = preferences.Theme ?? "none",
            ["timeframe"] = preferences.Timeframe ?? "none",
            ["anchorPreferences"] = preferences.AnchorPreferencesString,
            //["preferredEntities"] = preferences.EntityTypeString ?? "none",
            ["userLocation"] = locationPoint,
            ["currentExperienceJson"] = currentExperienceJson
        };
        var promptTemplate = new KernelPromptTemplateFactory().Create(new PromptTemplateConfig(Prompts.PrismAIAgentChatPrompt));
        var systemPrompt = await promptTemplate.RenderAsync(kernel, args);
        history.AddSystemMessage(systemPrompt);
        var chatService = kernel.Services.GetRequiredService<IChatCompletionService>();
        await foreach (var message in chatService.GetStreamingChatMessageContentsAsync(history, settings, kernel))
        {
            if (string.IsNullOrEmpty(message.Content)) continue;
            yield return message.Content;
        }

    }
    public Task Cancel()
    {
        return Task.CompletedTask;
    }

    private async Task<WebFindResult> GetWebFindResult(Recommendation recommendation, string connectionId)
    {
        Console.WriteLine($"GetWebAndVideoRecommendations called");
        var kernel = CreateKernel(connectionId);
        //var qloo = kernel.ImportPluginFromType<QlooPlugin>();
        var googleSearch = new GoogleSearchService(_loggerFactory, _configuration);
        var webCrawlPlugin = new WebCrawlPlugin(googleSearch, _loggerFactory, _configuration);
        var webPlugin = kernel.ImportPluginFromObject(webCrawlPlugin);
        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), ResponseFormat = typeof(WebFindResult) };
        var args = new KernelArguments(settings) { ["title"] = recommendation.Title, ["description"] = recommendation.Description, ["type"] = recommendation.Type };
        var response = await kernel.InvokePromptAsync<string>(Prompts.WebSummaryAgentPrompt, args);
        Console.WriteLine($"\n==========================\nRaw LLM Response from GetWebAndVideoRecommendations\n==========================\n{response}");
        return JsonSerializer.Deserialize<WebFindResult>(response)!;
    }
    private async Task<YoutubeMusicFindResults> GetYoutubeMusicResult(Recommendation recommendation, string connectionId)
    {
        Console.WriteLine($"GetYoutubeMusicResult called");
        var kernel = CreateKernel(connectionId);
        //var qloo = kernel.ImportPluginFromType<QlooPlugin>();
        var youtubeService = new YouTubePlugin(_configuration["YouTube:ApiKey"]!);
        var youtubePlugin = kernel.ImportPluginFromObject(youtubeService);
        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), ResponseFormat = typeof(YoutubeMusicFindResults) };
        var args = new KernelArguments(settings) { ["title"] = recommendation.Title, ["description"] = recommendation.Description, ["type"] = recommendation.Type };
        var response = await kernel.InvokePromptAsync<string>(Prompts.YoutubeMusicRecommendationAgentPrompt, args);
        Console.WriteLine($"\n==========================\nRaw LLM Response from GetYoutubeMusicResult\n==========================\n{response}");
        return JsonSerializer.Deserialize<YoutubeMusicFindResults>(response)!;
    }
    private async Task<YoutubeGeneralFindResults> GetYoutubeSearchResult(Recommendation recommendation, string connectionId)
    {
        Console.WriteLine("GetYoutubeSearchResult called");
        var kernel = CreateKernel(connectionId);
        //var qloo = kernel.ImportPluginFromType<QlooPlugin>();
        var youtubeService = new YouTubePlugin(_configuration["YouTube:ApiKey"]!);
        var youtubePlugin = kernel.ImportPluginFromObject(youtubeService);
        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), ResponseFormat = typeof(YoutubeGeneralFindResults) };
        var args = new KernelArguments(settings) { ["title"] = recommendation.Title, ["description"] = recommendation.Description, ["type"] = recommendation.Type };
        var response = await kernel.InvokePromptAsync<string>(Prompts.YoutubeGeneralVideoRecommenderAgentPrompt, args);
        Console.WriteLine($"\n==========================\nRaw LLM Response from GetYoutubeSearchResult\n==========================\n{response}");
        return JsonSerializer.Deserialize<YoutubeGeneralFindResults>(response)!;
    }

    private async Task<BookFindResults> GetBookFindResults(Recommendation recommendation, string connectionId)
    {
        Console.WriteLine($"GetBookFindResults called");
        var kernel = CreateKernel(connectionId);
        var bookService = new BookPlugin(_configuration["Google:Books:ApiKey"]!);
        var bookPlugin = kernel.ImportPluginFromObject(bookService);
        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), ResponseFormat = typeof(BookFindResults) };
        var args = new KernelArguments(settings) { ["title"] = recommendation.Title, ["description"] = recommendation.Description, ["type"] = recommendation.Type };
        var response = await kernel.InvokePromptAsync<string>(Prompts.BookRecommenderAgentPrompt, args);
        Console.WriteLine($"\n==========================\nRaw LLM Response from GetBookFindResults\n==========================\n{response}");
        return JsonSerializer.Deserialize<BookFindResults>(response)!;
    }
    private Kernel CreateKernel(string connectionId, string modelId = "gpt-4.1", string location = "")
    {
        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, _configuration["OpenAI:ApiKey"]!);
        if (modelId.Contains("deepseek"))
        {
            var url = new Uri("https://openrouter.ai/api/v1/");
            var client = DelegateHandlerFactory.GetHttpClientWithHandler<LoggingHandler>();
            kernelBuilder.AddOpenAIChatCompletion(modelId, url, _configuration["OpenRouter:ApiKey"], httpClient: client);
        }
        else
        {
            kernelBuilder.AddOpenAIChatCompletion(modelId, _configuration["OpenAI:ApiKey"]!);
        }
        kernelBuilder.Services.AddLogging(o =>
        {
            o.AddConsole();
        });
        kernelBuilder.Services.AddSingleton(_qlooService);
        var kernel = kernelBuilder.Build();
        kernel.Data["connectionId"] = connectionId;
        kernel.Data["location"] = location;
        kernel.AutoFunctionInvocationFilters.Add(_autoInvokeFilter);
        return kernel;
    }
    private async Task<RouterResponse> RouteRequestAsync(Recommendation recommendation)
    {
        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-4.1-mini", _configuration["OpenAI:ApiKey"]!);
        kernelBuilder.Services.AddLogging();
        var kernel = kernelBuilder.Build();
        var settings = new OpenAIPromptExecutionSettings() { ResponseFormat = typeof(RouterResponse) };
        var args = new KernelArguments(settings) { ["title"] = recommendation.Title, ["description"] = recommendation.Description, ["type"] = recommendation.Type };
        var response = await kernel.InvokePromptAsync<string>(Prompts.FindMoreRouterAgentPrompt, args);
        Console.WriteLine($"\n==========================\nRaw LLM Response from Router\n==========================\n{response}");
        return JsonSerializer.Deserialize<RouterResponse>(response)!;
    }
    private class RouterResponse
    {
        [Description("Agent to route to")]
        public required AgentType AgentType { get; set; }
    }
    [JsonConverter(typeof(JsonStringEnumConverter<AgentType>))]
    private enum AgentType { WebRecommender, BookRecommender, MusicRecommender, YoutubeSearch }
}