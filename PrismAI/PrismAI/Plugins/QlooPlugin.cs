using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PrismAI.Core.Models;
using PrismAI.Core.Models.Attributes;
using PrismAI.Core.Models.Helpers;
using PrismAI.Core.Models.RequestModels;
using PrismAI.Core.Models.ResponseModels;
using PrismAI.Core.Services;
using PrismAI.Maps;

namespace PrismAI.Plugins;

public class QlooPlugin
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    [KernelFunction, Description("Retrieve tag/interests metadata associated with a certain audience, entity, tag, or keyword. This is referred to as taste analysis")]
    public async Task<string> CallQlooTastes([FromKernelServices] IQlooService service, [Description("Parameters used to narrow down the results based on criteria such as type, popularity, and tags.")] TasteAnalysisRequest tasteAnalysisRequest)
    {
        
        try
        {
            var response = await service.GetTastesAnalysis(tasteAnalysisRequest);
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling Qloo Tastes API: {ex.Message}");
            return $"Error: {ex.Message}. Make the necessary adjustments and try again.";
        }
    }
    [KernelFunction, Description("Retrieve demographic data for an entity. Once you include the required parameters, you'll get a response containing the affinity score for various types of demographic data, like age and gender.")]
    public async Task<string> CallQlooDemographicInsights([FromKernelServices] IQlooService service, [Description("Request body for api request.")] InsightsRequestModels insightsRequestModel)
    {
        try
        {
            var insightsRequest = insightsRequestModel.InsightsRequest;
            insightsRequest.Filter.FilterType = FilterType.Demographics;
            var response = await service.GetInsightsAsync(insightsRequest);
            return JsonSerializer.Serialize(response, _jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling Qloo Tastes API for DemographicInsights: {ex.Message}");
            return $"Error: {ex.Message}. Make the necessary adjustments and try again.";
        }
    }
    [KernelFunction, Description("Call Qloo Insights Api which 'unlocks deep, semantic insight into how people interact with the world around them — from music, TV, dining, fashion, and travel to brands, books, podcasts, and more'")]
    public async Task<string> CallQlooEntityInsights(Kernel kernel, [FromKernelServices] IQlooService service, [Description("Parameters used to narrow down the results based on criteria such as type, popularity, and tags.")] FilterParams filterParams,  [Description(EntityTypeDescription)] EntityType entityType, [Description("Parameters that influence recommendations by weighting factors such as demographics, biases, and user interests.")] SignalParams signal, [Description("Parameters used to control the output, including the pagination of results.")] OutputParams? outputParams = null)
    {
        var location = "";
        if (kernel.Data.TryGetValue("location", out var locationObj))
            location = locationObj as string ?? "";
        var age = "";
        if (kernel.Data.TryGetValue("age", out var ageObj))
            age = ageObj as string ?? "";
        var gender = "";
        if (kernel.Data.TryGetValue("gender", out var genderObj))
            gender = genderObj as string ?? "";
        if (!string.IsNullOrEmpty(filterParams.Tags) && !string.IsNullOrEmpty(signal.InterestsTags))
        {
            var filterTags = filterParams.Tags;
            var signalTags = signal.InterestsTags;
            signal.InterestsTags = $"{filterTags},{signalTags}";
            filterParams.Tags = null;
        }
        else if (!string.IsNullOrEmpty(filterParams.Tags))
        {
            var filterTags = filterParams.Tags;
            signal.InterestsTags = filterTags;
            filterParams.Tags = null;
        }

        if (!string.IsNullOrEmpty(gender) && entityType != EntityType.Movie)
        {
            signal.DemographicsGender = gender;
            signal.DemographicsGenderWeight = "high";
        }
        if (!string.IsNullOrEmpty(age))
        {
            signal.DemographicsAge = age;
            signal.DemographicsAgeWeight = "high";
        }
        var interestArray = signal.InterestsTags?.Split(',').Distinct().ToList();
        var uuidRegex = new Regex(@"^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$");
        var entitiesArray = interestArray?.Where(x => uuidRegex.IsMatch(x)).ToList();
        interestArray = interestArray?.Where(x => !uuidRegex.IsMatch(x)).ToList();
        signal.InterestsTags = string.Join(',', interestArray ?? []);
        signal.InterestsEntities = string.Join(',', entitiesArray ?? []);
        try
        {
            var insightsRequest = new InsightsRequest()
            {
                Filter = filterParams,
                Signal = signal,
                Output = outputParams
            };
            insightsRequest.Filter.FilterType = FilterType.Entities;
            if (insightsRequest.Filter.EntityType == EntityType.None)
            {
                insightsRequest.Filter.EntityType = entityType;
            }
            if (insightsRequest.Filter.EntityType is EntityType.Destination && !string.IsNullOrEmpty(location))
            {
                insightsRequest.Filter.Location ??= location;
                insightsRequest.Filter.LocationRadius ??= 300000;
            }

            var hasDestination = kernel.Data.ContainsKey("hasDestination");
            if (insightsRequest.Filter.EntityType is EntityType.Place && !hasDestination)
            {
                if (insightsRequest.Filter?.Location?.Contains("POINT") == true)
                {
                    insightsRequest.Filter.LocationRadius ??= 20000;
                }
            }
            var response = await service.GetInsightsAsync(insightsRequest, true);
            return JsonSerializer.Serialize(response, _jsonSerializerOptions);
        }
        catch (InvalidOperationException exception)
        {
            Console.WriteLine($"Error calling Qloo Tastes API: {exception.Message}");
            return $"Error: {exception.Message}. Modify the signal or filter parameters and try again.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling Qloo Tastes API: {ex.Message}");
            return $"Error: {ex.Message}. Make the necessary adjustments and try again.";
        }
    }
    private const string EntityTypeDescription =
        """
        The entity type for which to retrieve insights. Must be one of:
        - `Artist`
        - `Book`
        - `Brand`
        - `Destination`
        - `Movie`
        - `Person`
        - `Place`
        - `Podcast`
        - `TvShow`
        - `VideoGame`
        """;
    //[KernelFunction, Description(HeatmapRules)]
    //public async Task<string> GenerateHeatmap([FromKernelServices] IQlooService service, [Description("Request body for api request.")] InsightsRequest insightsRequest)
    //{
    //    try
    //    {
    //        var response = await service.GetInsightsAsync(insightsRequest);
    //        if (response?.Results?.Heatmap == null)
    //        {
    //            return "No data available for heatmap generation.";
    //        }

    //        var heatmapData = response.Results.ToHeatmapResult();
    //        return JsonSerializer.Serialize(heatmapData, _jsonSerializerOptions);

    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Error generating heatmap: {ex.Message}");
    //        return $"Error: {ex.Message}. Make the necessary adjustments and try again.";
    //    }
    //}

    //[KernelFunction, Description(LocationsMapRules)]
    //public async Task<string> GenerateLocationsMap(Kernel kernel, [FromKernelServices] IQlooService service, [Description("Request body for api request.")] InsightsRequestModels insightsRequestModel, [Description("A cleaned up representation of the original user query")] string userRequestDescription, [Description("The maximum number of heatmap points that should be used for a locations search. This will depend on the percieved area of the expected map. Valid values are between 5 and 100")] int maxPoints = 10)
    //{
    //    try
    //    {
    //        var insightsRequest = insightsRequestModel.InsightsRequest;
    //        var response = await service.GetInsightsAsync(insightsRequest);
    //        if (response?.Results?.Heatmap == null)
    //        {
    //            return "No data available for locations generation.";
    //        }

    //        if (!response.Success)
    //        {
    //            var errorBuilder = new StringBuilder();
    //            errorBuilder.AppendLine("Error generating locations map:");
    //            foreach (var error in response.Errors ?? [])
    //            {
    //                errorBuilder.AppendLine($"- {error}");
    //            }
    //        }

    //        var heatmapData = response.Results.ToHeatmapResult();
    //        var json = JsonSerializer.Serialize(heatmapData, _jsonSerializerOptions);
    //        var settings = new OpenAIPromptExecutionSettings() { ResponseFormat = typeof(PlacesSearchModel) };
    //        var args = new KernelArguments(settings)
    //        {
    //            ["heatMapJson"] = json,
    //            ["query"] = userRequestDescription,
    //            ["maxPoints"] = maxPoints
    //        };
    //        var responseText = await kernel.InvokePromptAsync<string>(ConvertToPlacesPrompt, args);
    //        var length = Math.Min(responseText.Length - 1, 1000);
    //        Console.WriteLine($"\n========================================\nLocations Map Response:\n========================================\n {responseText[..length]}");
    //        var placesSearch = JsonSerializer.Deserialize<PlacesSearchModel>(responseText);
    //        return JsonSerializer.Serialize(placesSearch, _jsonSerializerOptions);


    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Error generating heatmap: {ex.Message}");
    //        return $"Error: {ex.Message}. Make the necessary adjustments and try again.";
    //    }
    //}

    [KernelFunction, Description("Get the available unique entity ids.")]
    public async Task<string> SearchForEntities([FromKernelServices] IQlooService service, [Description("The unique entity")] string entityQuery)
    {
        var response = await service.SearchForEntities(entityQuery);
        var length = Math.Min(response.Length - 1, 1000);
        Console.WriteLine($"\n========================================\nSearch for entities response:\n========================================\n {response[..length]}");
        return response;
    }
    [KernelFunction, Description("Get the available unique tag ids.")]
    public async Task<string> SearchForTags([FromKernelServices] IQlooService service, [Description("The unique entity for which you need a tag id")] string tagQuery)
    {
        var response = await service.GetTagSearchResults(tagQuery);
        var json = JsonSerializer.Serialize(response, _jsonSerializerOptions);
        var length = Math.Min(json.Length - 1, 1000);
        Console.WriteLine($"\n========================================\nSearch for tags response:\n========================================\n");
        return json;
    }

    [KernelFunction, Description(AudienceSearchTypeDescription)]
    public async Task<string> GetAudiences([FromKernelServices] IQlooService service)
    {
        var audienceData = FileHelpers.ExtractFromAssembly<AudienceSearchResponse>("Audiences.json");
        var simplified = audienceData.SimpleAudienceData.Select(x => new { x.Name, x.AudienceEntityId, x.AudienceTagId });
        var json = JsonSerializer.Serialize(simplified, _jsonSerializerOptions);
        return json;
    }
    
    [KernelFunction, Description("Get the allowed attributes for specific entity type Qloo queries")]
    public string GetAllowedParametersForEntityType([Description("The entity type for which to retrieve allowed parameters.")] EntityType entityType)
    {
        var attributes = entityType.GetType()
            .GetField(entityType.ToString())
            ?.GetCustomAttributes(typeof(AllowedParametersAttribute), false)
            .FirstOrDefault() as AllowedParametersAttribute;
        var result = "";
        if (attributes == null)
        {
            result = $"{entityType} is not a valid entity type";
            return result;
        }
        result = attributes.EntityTypeParameter;
        Console.WriteLine($"Allowed parameters for {entityType}: {result.Replace(",", ", ")}");
        return result;
    }
    [KernelFunction, Description("Add notes to a scratchpad.")]
    public void AddToScratchpad([FromKernelServices] IQlooService service, [Description("The text to add to the scratchpad.")] string text)
    {

    }
    [KernelFunction, Description("Find a related alternative to a recommenddation")]
    public async Task<string> FindRelatedAlternativeEntities([FromKernelServices] IQlooService service, [Description("The recommendation to find an alternative for.")] AnalysisQuery analysisQueryParams)
    {
        try
        {
            var analysisQuery = analysisQueryParams.Parameters;
            var response = await service.GetAnalysisResponse(analysisQuery);
            if (response == null)
            {
                return "No related alternative found.";
            }
            var json = JsonSerializer.Serialize(response, _jsonSerializerOptions);
            var length = Math.Min(json.Length - 1, 1000);
            Console.WriteLine($"\n========================================\nFind Related Alternative Response:\n========================================\n {json[..length]}");
            return json;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding related alternative: {ex.Message}");
            return $"Error: {ex.Message}. Make the necessary adjustments and try again.";
        }
    }
    [KernelFunction,Description("Get The tag types for a Qloo taste analysis request.")]
    public async Task<string> GetTagTypes([FromKernelServices] IQlooService service)
    {
        try
        {
            //var compareQuery = entityCompareQuery.ComparisonParameters;
            var response = await service.GetAllTagTypesAsync();
            //return JsonSerializer.Serialize(response.Results);
            return response.AsMarkdown;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error comparing entities: {ex.Message}");
            return $"Error: {ex.Message}. Make the necessary adjustments and try again.";
        }
    }
    private const string AudienceSearchTypeDescription =
        """
        The Find Audiences API retrieves a list of audience IDs that can be used for filtering results and refining targeting in recommendations. You can use the returned audience IDs as values for `signal.demographics.audiences` to filter Insights API query results by specific audiences.
        """;
}