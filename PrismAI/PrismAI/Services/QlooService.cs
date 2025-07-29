using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using PrismAI.Core.Models;
using PrismAI.Core.Models.Attributes;
using PrismAI.Core.Models.CultureConciergeModels;
using PrismAI.Core.Models.Helpers;
using PrismAI.Core.Models.RequestModels;
using PrismAI.Core.Models.ResponseModels;
using PrismAI.Core.Services;

//using static System.Net.WebRequestMethods;

namespace PrismAI.Services;

public class QlooService(ILoggerFactory loggerFactory, HttpClient httpClient) : IQlooService
{
    private const string BaseUrl = "https://hackathon.api.qloo.com";
    private const string StartBlue = "\e[34m";
    private const string EndBlue = "\e[0m";
    private ILogger<QlooService> _logger = loggerFactory.CreateLogger<QlooService>();

    public async Task<InsightsResponse> GetInsightsAsync(InsightsRequest request,
        bool requireEntities = false, CancellationToken cancellationToken = default)
    {
        //if (!InsightsRequestValidator.TryValidate(request, out var errors))
        //{
        //    Console.WriteLine($"{string.Join("\n", errors)}");
        //}
        _queryStringBuilder = new StringBuilder();
        var queryString = ToQueryString(request);
        //File.WriteAllText($"QueryString-{Guid.NewGuid().ToString()}.txt", _queryStringBuilder.ToString());
        var url = $"{BaseUrl}/v2/insights/?{queryString}";
        Console.WriteLine($"\n================================\nGET Query String\n===============================\n{StartBlue}{url}{EndBlue}\n===============================\n");
        
        var getResponse = await httpClient.GetAsync(url, cancellationToken);
        string responseJson = await getResponse.Content.ReadAsStringAsync(cancellationToken);
        var length = Math.Min(responseJson.Length-1, 1000);
        Console.WriteLine($"\n================================\nRaw Json Result\n===============================\n{responseJson[..length]}");
        //File.WriteAllText("Insights.json", responseJson);
        var data = JsonSerializer.Deserialize<InsightsResponse>(responseJson);
        if (data == null || data?.Success == false)
        {
            _logger.LogError("Failed to deserialize InsightsResponse from JSON.");
            throw new JsonException("Deserialization of InsightsResponse failed.");
        }
        if (requireEntities && (data.Results?.Entities == null || data.Results.Entities.Count == 0))
        {
            _logger.LogWarning("No entities found in the InsightsResponse.");
            throw new InvalidOperationException("No entities found in the InsightsResponse.");
        }
        return data;
    }

    // Builds query string from flattened request object using JsonPropertyName attributes
    public static string ToQueryString(object obj, string prefix = "")
    {
        if (obj == null) return string.Empty;
        var query = new List<string>();
        var type = obj.GetType();
        
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = prop.GetValue(obj);
            if (value == null) continue;
            
            // Skip properties marked with JsonIgnore
            
            var jsonProp = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
            var queryStringPartAttribute = prop.GetCustomAttribute<QueryStringPartAttribute>()?.Name;
            if (string.IsNullOrEmpty(jsonProp) && string.IsNullOrEmpty(queryStringPartAttribute)) continue;
            
            var key = string.IsNullOrEmpty(prefix) ? jsonProp : $"{prefix}.{jsonProp}";
            key = !string.IsNullOrEmpty(queryStringPartAttribute) ? queryStringPartAttribute : key;
            switch (value)
            {
                case string s:
                    if (!string.IsNullOrWhiteSpace(s))
                        query.Add($"{key}={HttpUtility.UrlEncode(s)}");
                    break;
                case int intValue:
                    query.Add($"{key}={intValue}");
                    break;
                case bool boolValue:
                    query.Add($"{key}={HttpUtility.UrlEncode(boolValue.ToString().ToLower())}");
                    break;
                case double:
                case float:
                case long:
                    query.Add($"{key}={HttpUtility.UrlEncode(value.ToString())}");
                    break;
                case IEnumerable<string> stringList:
                    // Join list as comma-separated
                    var joined = string.Join(",", stringList);
                    if (!string.IsNullOrWhiteSpace(joined))
                        query.Add($"{key}={HttpUtility.UrlEncode(joined)}");
                    break;
                default:
                    // For complex objects, recursively process them
                    if (value.GetType().IsClass)
                    {
                        var nested = ToQueryString(value, key);
                        if (!string.IsNullOrWhiteSpace(nested))
                            query.Add(nested);
                    }
                    break;
            }

            if (prop.Name == "Type")
            {
                Console.WriteLine($"Query after Type value parsed: {string.Join("&", query)}");
            }
        }

        var queryString = string.Join("&", query);
        _queryStringBuilder.Append(queryString);
        return queryString;
    }
    private static StringBuilder _queryStringBuilder = new();
    public async Task<TagSearchResult> GetTagSearchResults(string query)
    {
        var uriString = $"{BaseUrl}/v2/tags?feature.typo_tolerance=true&filter.query={query}";
        var tags = await httpClient.GetStringAsync(new Uri(uriString));
        var length = Math.Min(tags.Length - 1, 1000);
        Console.WriteLine(
            $"Fetched tags from {StartBlue}{uriString}{EndBlue}\n================================\n");
        var tagSearchResult = JsonSerializer.Deserialize<TagSearchResult>(tags);
        return tagSearchResult;
    }
    public Task<TagsEntityResult> GetAllTagTypesAsync()
    {
        return Task.FromResult(FileHelpers.ExtractFromAssembly<TagsEntityResult>("Tags.json"));
        
    }

    public async Task<AudiencesResult> GetAllAudienceCategoriesAsync()
    {
        var result = new AudiencesResult();
        var page = 1;
        while (true)
        {
            try
            {
                var audiencesJson = await httpClient.GetStringAsync(new Uri($"{BaseUrl}/v2/audiences/types?page={page}&take=50"));
                Console.WriteLine($"Fetched audiences from {BaseUrl}/v2/audiences/types?page={page}&take=50\n================================\n{audiencesJson}\n================================\n");
                var entityResponse = JsonSerializer.Deserialize<AudiencesResult>(audiencesJson);
                if (entityResponse.Results.AudienceTypes?.Count == 0)
                {
                    break; // No more audiences to fetch
                }
                if (page == 1)
                {
                    result = entityResponse;
                }
                else
                {
                    result.Results.AudienceTypes.AddRange(entityResponse.Results.AudienceTypes);
                    result.Duration += entityResponse.Duration;
                }
                page++;
            }
            catch
            {
                break;
            }
        }
        await File.WriteAllTextAsync("Audiences.json", JsonSerializer.Serialize(result, new JsonSerializerOptions() { WriteIndented = true }));
        return result;
    }

    public async Task<AudienceSearchResponse> SearchForAudienceAsync(AudienceSearchRequest request)
    {
        var queryString = ToQueryString(request);
        var url = $"{BaseUrl}/v2/audiences?{queryString}";
        Console.WriteLine($"\n================================\nGET Audience Types Query String\n===============================\n{url}");
        var audiencesResponseString = await httpClient.GetFromJsonAsync<AudienceSearchResponse>(url);

        return audiencesResponseString;
    }
    public async Task<string> SearchForEntities(string query)
    {
        var entitiesResponseMessage = await httpClient.GetAsync(new Uri($"{BaseUrl}/search?query={query}"));
        var entitiesJson = await entitiesResponseMessage.Content.ReadAsStringAsync();
        var length = Math.Min(entitiesJson.Length - 1, 1000);
        Console.WriteLine($"Fetched Entities from '{BaseUrl}/search?query={query}'\n================================\n{entitiesJson[..length]}\n================================\n");
        return entitiesJson;
    }

    public async Task<string> ComareEntities(InsightsComparisonQuery entityCompareQuery)
    {
        var queryString = ToQueryString(entityCompareQuery);
        var url = $"{BaseUrl}/v2/insights/compare?{queryString}";
        Console.WriteLine($"\n================================\nGET Compare Entities Query String\n===============================\n{StartBlue}{url}{EndBlue}");
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to compare entities. Status code: {response.StatusCode}");
            return string.Empty;
        }
        var responseJson = await response.Content.ReadAsStringAsync();
        return responseJson;
    }

    public async Task<string> GetTastesAnalysis(TasteAnalysisRequest tasteAnalysisRequest)
    {
        var queryString = ToQueryString(tasteAnalysisRequest);
        var url = $"{BaseUrl}/v2/insights?{queryString}";
        Console.WriteLine($"\n================================\nGET Compare Entities Query String\n===============================\n{StartBlue}{url}{EndBlue}");
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed Taste analysis request. Status code: {response.StatusCode}");
            return string.Empty;
        }
        var responseJson = await response.Content.ReadAsStringAsync();
        return responseJson;
    }
    //public async Task<EntityTrendsResponse> GetTrendingEntities(string entityType, int page = 1, int take = 5)
    //{
    //    var url = $"{BaseUrl}/trends/category?type={entityType}&page={page}&take={take}";
        
    //    Console.WriteLine($"\n================================\nGET Trending Entities Query String\n===============================\n{url}");
    //    var response = await httpClient.GetFromJsonAsync<EntityTrendsResponse>(url);
    //    return response;
    //}

    public async Task<AnalysisResponse> GetAnalysisResponse(AnalysisQueryParameters analysisQuery)
    {
        var url = $"{BaseUrl}/analysis?{analysisQuery.ToQueryString()}";
        Console.WriteLine($"\n================================\nGET Analysis Query String\n===============================\n{StartBlue}{url}{EndBlue}");
        var response = await httpClient.GetFromJsonAsync<AnalysisResponse>(url);
        return response;
    }
}