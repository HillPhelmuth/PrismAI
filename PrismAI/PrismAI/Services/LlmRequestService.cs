using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PrismAI.Core.Models;
using PrismAI.Core.Models.CultureConciergeModels;
using PrismAI.Core.Models.Helpers;
using PrismAI.Core.Services;
using PrismAI.Hubs;
using PrismAI.Maps;
using PrismAI.Plugins;
using PrismAI.Services.HttpHandlers;

namespace PrismAI.Services;

public class LlmRequestService : ILlmRequestService, IAiAgentService
{
    private readonly AutoInvokeFilter _autoInvokeFilter = new();
    private readonly IConfiguration _configuration;
    private readonly IQlooService _qlooService;
    public event Action<HeatmapResult>? HeatmapGenerated;
    private readonly IHubContext<EventHub> _hubContext;
    private readonly ILoggerFactory _loggerFactory;
    public LlmRequestService(IConfiguration configuration, IQlooService qlooService, IHubContext<EventHub> hubContext, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _qlooService = qlooService;
        _hubContext = hubContext;
        _loggerFactory = loggerFactory;
        _autoInvokeFilter.HeatmapGenerated += OnHeatmapGenerated;
        _autoInvokeFilter.LocationsMapGenerated += HandleLocationsMapGenerated;
        _autoInvokeFilter.AutoFunctionInvocationStarted += OnAutoFunctionInvocationStarted;
        _autoInvokeFilter.AutoFunctionInvocationCompleted += OnAutoFunctionInvocationCompleted;
        _autoInvokeFilter.DemographicInsightsGenerated += HandleDemographicInsightsGenerated;
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

    private DemographicsChartDto? _demographicInsightsResponse;
    private CancellationTokenSource _cancellationTokenSource = new();
    public event Action<string>? OnFunctionInvoked;
    public event Action<string>? OnFunctionInvocationCompleted;
    public event Action<DemographicsChartDto>? OnDemographicInsightsGenerated;
    public event Action<Experience>? OnExperienceUpdated;
    private void HandleDemographicInsightsGenerated(DemographicsChartDto obj, string connectionId)
    {
        _demographicInsightsResponse = obj;
        Console.WriteLine($"Demographic insights Handled from Function Filter: {JsonSerializer.Serialize(obj)}");
        if (!string.IsNullOrEmpty(connectionId))
            _hubContext.Clients.Client(connectionId).SendAsync("DemographicInsightsGenerated", obj);
    }

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

    private void HandleLocationsMapGenerated(PlacesSearchModel obj)
    {
        _hubContext.Clients.All.SendAsync("LocationsMapGenerated", obj);
    }

    private void OnHeatmapGenerated(HeatmapResult heatMap)
    {
        _hubContext.Clients.All.SendAsync("HeatmapGenerated", heatMap);
    }

    public async Task<InsightsRequest> FormulateRequestAsync(QueryRequest query)
    {
        Console.WriteLine($"Formulating request for query: {query.Query}");
        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion("o3", _configuration["OpenAI:ApiKey"]);
        kernelBuilder.Services.AddLogging();
        kernelBuilder.Services.AddSingleton(_qlooService);
        var kernel = kernelBuilder.Build();
        kernel.AutoFunctionInvocationFilters.Add(_autoInvokeFilter);
        var settings = new OpenAIPromptExecutionSettings() { ResponseFormat = typeof(InsightsRequest) };
        var args = new KernelArguments(settings) { ["user_question"] = query.Query };
        var response = await kernel.InvokePromptAsync<string>(Prompts.EntityRequestAgentPrompt, args);
        Console.WriteLine($"\n==========================\nRaw LLM Response\n==========================\n{response}");
        return JsonSerializer.Deserialize<InsightsRequest>(response);
    }
    public async Task<string> FunctionCallRequest(QueryRequest query)
    {
        Console.WriteLine($"Function call request for query: {query.Query}");
        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-4.1", _configuration["OpenAI:ApiKey"]);
        kernelBuilder.Services.AddLogging();
        kernelBuilder.Services.AddSingleton(_qlooService);
        var kernel = kernelBuilder.Build();
        kernel.ImportPluginFromType<QlooPlugin>();
        var googleSearch = new GoogleSearchService(_loggerFactory, _configuration);
        var webCrawlPlugin = new WebCrawlPlugin(googleSearch, _loggerFactory, _configuration);
        kernel.ImportPluginFromObject(webCrawlPlugin);
        kernel.AutoFunctionInvocationFilters.Add(_autoInvokeFilter);
        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        var args = new KernelArguments(settings) { ["user_question"] = query.Query };
        var response = await kernel.InvokePromptAsync<string>(Prompts.FunctionCallPrompt, args);
        var length = Math.Min(response.Length - 1, 1000);
        Console.WriteLine($"\n==========================\nRaw LLM Response\n==========================\n{response[..length]}");
        return response;
    }

    public async Task<string> InteractiveAgentChat(ChatHistory history, string creatorBrief)
    {
        var enginer = new KernelPromptTemplateFactory().Create(new PromptTemplateConfig(Prompts.ContentCreatorPrompt));
        var args = new KernelArguments() { ["CREATOR_BRIEF"] = creatorBrief };

        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-4.1", _configuration["OpenAI:ApiKey"]);
        kernelBuilder.Services.AddLogging();
        kernelBuilder.Services.AddSingleton(_qlooService);
        var kernel = kernelBuilder.Build();
        kernel.ImportPluginFromType<QlooPlugin>();
        var googleSearch = new GoogleSearchService(_loggerFactory, _configuration);
        var webCrawlPlugin = new WebCrawlPlugin(googleSearch, _loggerFactory, _configuration);
        kernel.ImportPluginFromObject(webCrawlPlugin);
        kernel.AutoFunctionInvocationFilters.Add(_autoInvokeFilter);
        var systemPrompt = await enginer.RenderAsync(kernel, args);
        history.AddSystemMessage(systemPrompt);
        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        var chatService = kernel.Services.GetRequiredService<IChatCompletionService>();
        var response = await chatService.GetChatMessageContentAsync(history, settings, kernel);
        return response.ToString();
    }

    public async Task<(AudienceAnalysisResult, DemographicsChartDto?)> GenerateAnalysisResult(CreativeBrief creativeBrief, string connection)
    {
        Console.WriteLine($"Function call request for query: {JsonSerializer.Serialize(creativeBrief, new JsonSerializerOptions() { WriteIndented = true })}");
        var modelId = "gpt-4.1";
        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, _configuration["OpenAI:ApiKey"]);
        kernelBuilder.Services.AddLogging();
        kernelBuilder.Services.AddSingleton(_qlooService);
        var kernel = kernelBuilder.Build();
        kernel.Data["connectionId"] = connection;
        var qloo = kernel.ImportPluginFromType<QlooPlugin>();
        Console.WriteLine($"Qloo plugin functions:\n=================================\n{string.Join("\n", qloo.Select(x => x.Name))}");
        //var googleSearch = new GoogleSearchService(_loggerFactory, _configuration);
        //var webCrawlPlugin = new WebCrawlPlugin(googleSearch, _loggerFactory, _configuration);
        //kernel.ImportPluginFromObject(webCrawlPlugin);
        kernel.AutoFunctionInvocationFilters.Add(_autoInvokeFilter);
        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new FunctionChoiceBehaviorOptions() { AllowConcurrentInvocation = true, AllowParallelCalls = true }), ResponseFormat = typeof(AudienceAnalysisResult) };
        var args = new KernelArguments(settings) { [nameof(CreativeBrief.Topic)] = creativeBrief.Topic, [nameof(CreativeBrief.ContentType)] = creativeBrief.ContentType, [nameof(CreativeBrief.TargetAudience)] = creativeBrief.TargetAudience, [nameof(CreativeBrief.CulturalReferences)] = creativeBrief.CulturalReferences, [nameof(CreativeBrief.AdditionalContext)] = creativeBrief.AdditionalContext };
        var response = await kernel.InvokePromptAsync<string>(Prompts.GenerateInsightsPrompt2, args);
        var length = Math.Min(response.Length - 1, 1000);
        //Console.WriteLine($"\n==========================\nRaw LLM Response\n==========================\n{response[..length]}");
        return (JsonSerializer.Deserialize<AudienceAnalysisResult>(response), _demographicInsightsResponse);
    }


    public async Task<Experience> GetExperienceRecommendations(UserPreferences preferences, UserProfile userProfile,
        string connectionId,
        string locationPoint = "", CancellationToken token = default)
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

        //var plugin = KernelPluginFactory.CreateFromFunctions("ExperiencePlugin", agent.Functions);
        //kernel.Plugins.Add(plugin);
        //kernel.AutoFunctionInvocationFilters.Add(_autoInvokeFilter);
        List<string> unneededFunctions = ["GenerateHeatmap", "GenerateLocationsMap", "CallQlooTastes",
            "CallQlooDemographicInsights"];
        var options = new FunctionChoiceBehaviorOptions()
        { AllowConcurrentInvocation = true, AllowParallelCalls = false };
        var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(functions: qloo.Where(x => !unneededFunctions.Any(y => x.Name.Contains(y))).Concat(webPlugin.Where(webFunc => webFunc.Name.Contains("SearchWebImages"))), options: options), ResponseFormat = typeof(Experience) };
        Console.WriteLine($"OpenAI Exec Settings:\n============================\n{JsonSerializer.Serialize(settings.Clone(), new JsonSerializerOptions() { WriteIndented = true })}\n============================\n");
        //var request = await FormulateEntityRequestAsync(preferences, connectionId, locationPoint, token);
        var allowedEntities = await GetEntityTypeResponse(preferences, connectionId, token);
        //var allowedGroups = allowedEntities.EntityTypeResponseItems.GroupBy(x => x.EntityType);
        var allowedEntityCountBuilder = new StringBuilder();
        foreach (var group in allowedEntities.EntityTypeResponseItems)
        {
            allowedEntityCountBuilder.AppendLine($"Number of {group.EntityType} recommendations - {group.NumberOfRecommendations}");
        }
        Console.WriteLine($"Allowed Entity String: {allowedEntityCountBuilder.ToString()}");
        var args = new KernelArguments(settings)
        {
            ["theme"] = preferences.Theme ?? "none",
            ["timeframe"] = preferences.Timeframe ?? "none",
            ["anchorPreferences"] = userProfile.UserInterestsString,
            //["preferredEntities"] = preferences.EntityTypeString ?? "none",
            ["preferredEntities"] = allowedEntityCountBuilder.ToString(),
            ["userLocation"] = locationPoint,
            ["partnerAnchorPreferences"] = string.IsNullOrEmpty(preferences.PartnerPreferencesString) ? "no partner" : preferences.PartnerPreferencesString

        };


        var response = await kernel.InvokePromptAsync<string>(Prompts.CultureConciergePrompt, args, cancellationToken: token);
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

    #region Experimental Partial Requests

    //private async Task<InsightsRequest[]> FormulateEntityRequestAsync(UserPreferences preferences, string connectionId,
    //    string locationPoint = "", CancellationToken token = default)
    //{
    //    var kernel = CreateKernel(connectionId);
    //    var qloo = kernel.ImportPluginFromType<QlooPlugin>();
    //    Console.WriteLine($"Qloo plugin functions:\n=================================\n{string.Join("\n", qloo.Select(x => x.Name))}");
    //    var googleSearch = new GoogleSearchService(_loggerFactory, _configuration);
    //    var webCrawlPlugin = new WebCrawlPlugin(googleSearch, _loggerFactory, _configuration);
    //    var webPlugin = kernel.ImportPluginFromObject(webCrawlPlugin);
    //    var youtubeService = new YouTubePlugin(_configuration["YouTube:ApiKey"]!);
    //    var youtubePlugin = kernel.ImportPluginFromObject(youtubeService);

    //    //var plugin = KernelPluginFactory.CreateFromFunctions("ExperiencePlugin", agent.Functions);
    //    //kernel.Plugins.Add(plugin);
    //    //kernel.AutoFunctionInvocationFilters.Add(_autoInvokeFilter);
    //    List<string> unneededFunctions = ["GenerateHeatmap", "GenerateLocationsMap", "CallQlooTastes",
    //        "CallQlooDemographicInsights"];
    //    var settings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(functions: qloo.Where(x => !unneededFunctions.Any(y => x.Name.Contains(y))).Concat(webPlugin.Where(webFunc => webFunc.Name.Contains("SearchWebImages")))), ResponseFormat = typeof(PartialInsightRequsts) };
    //    var args = new KernelArguments(settings)
    //    {
    //        ["theme"] = preferences.Theme ?? "none",
    //        ["timeframe"] = preferences.Timeframe ?? "none",
    //        ["anchorPreferences"] = preferences.AnchorPreferencesString,
    //        ["preferredEntities"] = preferences.EntityTypes is not null
    //            ? string.Join("\n\t- ", preferences.EntityTypes.Select(x => x.ToString()))
    //            : "none",
    //        ["userLocation"] = locationPoint,
    //    };
    //    var response = await kernel.InvokePromptAsync<string>(Prompts.GeneratEntityRequestPrompt, args, cancellationToken: token);
    //    Console.WriteLine($"\n==========================\nRaw LLM Response from FormulateEntityRequestAsync\n==========================\n{response}\n==========================\n");
    //    var insightsRequests = JsonSerializer.Deserialize<PartialInsightRequsts>(response);
    //    List<InsightsRequest> requestsResult = [];
    //    foreach (var insightsRequest in insightsRequests.Requsts)
    //    {
    //        insightsRequest.Filter.EntityType = insightsRequest.Type;
    //        insightsRequest.Filter.FilterType = FilterType.Entities;
    //        var result = new InsightsRequest()
    //        {
    //            Filter = insightsRequest.Filter,
    //            Signal = insightsRequest.Signal,
    //            Explainability = insightsRequest.Explainability,
    //            Output = insightsRequest.Output
    //        };
    //        requestsResult.Add(result);
    //    }

    //    return requestsResult.ToArray();
    //}

    private class PartialInsightRequsts
    {
        [Description("Create a minimum of 4 requests for different entity types.")]
        public required List<PartialInsightsRequest> Requsts { get; set; }
    }
    private class PartialInsightsRequest
    {
        [Description("The entity type for the request")]
        public required EntityType Type { get; set; }
        [JsonPropertyName("filter")]
        [Description("Filter parameters that constrain the query.")]
        public required FilterParams Filter { get; set; }
        [JsonPropertyName("signal")]
        [Description("Audience or context signals that influence affinity scoring, the primary way to get good, solid recommendations.")]
        public required SignalParams Signal { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [Description("Options controlling pagination, sorting, and explainability.")]
        public OutputParams? Output { get; set; }
        [JsonPropertyName("feature.explainability")]
        [Description("When set to true, the response includes explainability metadata for each recommendation and for the overall result set.")]
        public bool Explainability { get; set; }
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

    #endregion

    public async Task<Recommendation> GetAlternativeRecommendation(Recommendation recommendation, string connectionId,
        string location = "")
    {
        Console.WriteLine($"GetAlternativeRecommendation called with recommendation: {recommendation.Title}");
        var kernel = CreateKernel(connectionId);
        var qloo = kernel.ImportPluginFromType<QlooPlugin>();
        Console.WriteLine($"Qloo plugin functions:\n=================================\n{string.Join("\n", qloo.Select(x => x.Name))}");
        List<string> unneededFunctions = ["GenerateHeatmap", "GenerateLocationsMap", "CallQlooTastes",
            "CallQlooDemographicInsights" ];//"CallQlooEntityInsights" for non location or "FindRelatedAlternativeEntities" or locations
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
        var response = await kernel.InvokePromptAsync<string>(Prompts.ImageSearchPrompt, args);
        Console.WriteLine($"\n==========================\nRaw LLM Response from RequestImageSearch\n==========================\n{response}");
        return JsonSerializer.Deserialize<ImageResponse>(response)!;
    }

    public async IAsyncEnumerable<string> CultureConceirgeChat(ChatHistory history, Experience experience,
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
        var promptTemplate = new KernelPromptTemplateFactory().Create(new PromptTemplateConfig(Prompts.CultureUpdateChatPrompt));
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
        var response = await kernel.InvokePromptAsync<string>(Prompts.WebRecommenderPrompt, args);
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
        var response = await kernel.InvokePromptAsync<string>(Prompts.YoutubeMusicRecommenderPrompt, args);
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
        var response = await kernel.InvokePromptAsync<string>(Prompts.YoutubeGeneralVideoRecommenderPrompt, args);
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
        var response = await kernel.InvokePromptAsync<string>(Prompts.BookRecommenderPrompt, args);
        Console.WriteLine($"\n==========================\nRaw LLM Response from GetBookFindResults\n==========================\n{response}");
        return JsonSerializer.Deserialize<BookFindResults>(response)!;
    }
    private Kernel CreateKernel(string connectionId, string modelId = "gpt-4.1", string location = "")
    {
        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, _configuration["OpenAI:ApiKey"]!);
        if (modelId.Contains("gemini"))
        {
            var url = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/");
            var client = DelegateHandlerFactory.GetHttpClientWithHandler<LoggingHandler>();
            kernelBuilder.AddOpenAIChatCompletion(modelId, url, _configuration["Google:Gemini:ApiKey"], httpClient: client);
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
        var response = await kernel.InvokePromptAsync<string>(Prompts.FindMoreRouterPrompt, args);
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

public class AutoInvokeFilter : IAutoFunctionInvocationFilter
{
    public event Action<HeatmapResult>? HeatmapGenerated;
    public event Action<PlacesSearchModel>? LocationsMapGenerated;
    public event Action<AutoFunctionInvocationContext, string>? AutoFunctionInvocationStarted;
    public event Action<AutoFunctionInvocationContext, string>? AutoFunctionInvocationCompleted;
    public event Action<DemographicsChartDto, string>? DemographicInsightsGenerated;
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        var connectionId = context.Kernel.Data["connectionId"] as string;
        Console.WriteLine($"\n=============================\nFunction Called: {context.Function.Name}\n=============================\nconnection: {connectionId}\n=============================\n");
        
        AutoFunctionInvocationStarted?.Invoke(context, connectionId ?? "");
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var value = $"An error has occurred. Move on to a different request.\n\n{ex}";
            Console.WriteLine(value);
            context.Result = new FunctionResult(context.Function, value);
        }
        //if (!string.IsNullOrEmpty(connectionId))
        AutoFunctionInvocationCompleted?.Invoke(context, connectionId ?? "");
        if (context.Function.Name == "GenerateHeatmap")
        {
            var results = context.Result.ToString();
            var length = Math.Min(results.Length - 1, 1000);
            Console.WriteLine($"\n==========================\nHeatmap Result\n==========================\n{results[..length]}");
            var heatmapResult = JsonSerializer.Deserialize<HeatmapResult>(results);
            if (heatmapResult != null)
            {
                HeatmapGenerated?.Invoke(heatmapResult);
            }
            else
            {
                Console.WriteLine("Heatmap generation failed or returned null.");
            }
        }
        if (context.Function.Name == "GenerateLocationsMap")
        {
            var results = context.Result.ToString();
            var length = Math.Min(results.Length - 1, 1000);
            Console.WriteLine($"\n==========================\nLocations Map Result\n==========================\n{results[..length]}");
            var placesSearch = JsonSerializer.Deserialize<PlacesSearchModel>(results);
            if (placesSearch != null)
            {
                LocationsMapGenerated?.Invoke(placesSearch);
            }
            else
            {
                Console.WriteLine("Locations map generation failed or returned null.");
            }
        }

        if (context.Function.Name == "CallQlooDemographicInsights")
        {
            var results = context.Result.ToString();

            var insights = JsonSerializer.Deserialize<InsightsResponse>(results);
            var output = DemographicsChartDto.FromQlooDemographics(insights!.Results);
            DemographicInsightsGenerated?.Invoke(output, connectionId ?? "");
        }
    }
}