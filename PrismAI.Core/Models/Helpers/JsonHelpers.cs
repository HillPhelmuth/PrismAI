using System.Text.Json;

namespace PrismAI.Core.Models.Helpers;

public static class JsonHelpers
{
    public static bool TryDeserializeJson<T>(string json, out T? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(json);
            return result is not null;
        }
        catch (JsonException)
        {
            result = default;
            return false;
        }
    }
}