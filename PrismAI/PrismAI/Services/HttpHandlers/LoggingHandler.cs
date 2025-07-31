using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net.Http.Headers;

namespace PrismAI.Services.HttpHandlers;

public sealed class LoggingHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { WriteIndented = true };

    private const string Seperator = "\n==========================================================\n";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var isStream = false;
        if (request.Content is not null)
        {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                var root = JsonNode.Parse(requestBody);
                if (root != null)
                {
                    // Add or overwrite the "provider" property
                    root["provider"] = new JsonObject
                    {
                        ["only"] = new JsonArray("Cerebras")
                    };
                    isStream = root["stream"]?.GetValue<bool>() ?? false;

                    // Serialize the modified JSON back to the request content
                    var modifiedBody = root.ToJsonString();
                    request.Content = new StringContent(modifiedBody);
                    //request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    string formattedContent = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(modifiedBody), s_jsonSerializerOptions);
                    Console.WriteLine($"{Seperator}{request.RequestUri}{Seperator}=== REQUEST ===\n\n{formattedContent}{Seperator}");
                }
            }
            catch (JsonException)
            {
                Console.WriteLine(requestBody);
            }
        }

        // Call the next handler in the pipeline
        var responseMessage = await base.SendAsync(request, cancellationToken);

        if (isStream) return responseMessage;
        var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine($"{Seperator}=== RESPONSE ===\n{responseBody}{Seperator}");
        return responseMessage;
    }
}