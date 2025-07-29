using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PrismAI.Core.Models.Helpers;

public class GenericTypeConverter<T> : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => true;
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        //Console.WriteLine($"Converting {value} to {typeof(T)}");
        try
        {
            var json = value.ToString();
            //if (string.IsNullOrWhiteSpace(json)) return default(T);
            //var node = JsonNode.Parse(json);
            //if (node is JsonObject obj)
            //{
            //    ConvertArraysToCommaSeparatedStrings(obj);
            //    json = obj.ToJsonString();
            //}

            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting from {value} to {typeof(T).Name}: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    private void ConvertArraysToCommaSeparatedStrings(JsonObject obj)
    {
        foreach (var property in obj)
        {
            var key = property.Key;
            var value = property.Value;
            if (value is JsonArray arr && arr.All(e => e is JsonValue))
            {
                var strArr = arr.Select(e => e?.ToString()).ToArray();
                obj[key] = string.Join(",", strArr);
            }
            else if (value is JsonObject childObj)
            {
                ConvertArraysToCommaSeparatedStrings(childObj);
            }
        }
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        Console.WriteLine($"Converting {typeof(T)} to {value}");
        return JsonSerializer.Serialize(value);
    }
}