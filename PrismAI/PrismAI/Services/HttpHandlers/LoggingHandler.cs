using System.Text.Json;
using System.Text.Json.Nodes;

namespace PrismAI.Services.HttpHandlers;

public sealed class LoggingHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { WriteIndented = true };

    private const string Seperator = "\n==========================================================\n";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        
        //Console.WriteLine(request.RequestUri?.ToString());
        var isStream = false;
        if (request.Content is not null)
        {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                var root = JsonNode.Parse(requestBody);
                if (root != null)
                {
                    isStream = root["stream"]?.GetValue<bool>() ?? false;
                }
                string formattedContent = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(requestBody), s_jsonSerializerOptions);
                Console.WriteLine($"{Seperator}{request.RequestUri}{Seperator}=== REQUEST ===\n\n{formattedContent}{Seperator}");
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