using System.ComponentModel;
using PrismAI.Core.Models.PrismAIModels;

namespace PrismAI.Core.Models.Helpers;

public static class EnumExtensions
{
    public static string GetDescriptionAttribute(this Enum item)
    {
        // Get the value of the description attribute
        var type = item.GetType();
        var name = Enum.GetName(type, item);
        if (name == null)
        {
            throw new ArgumentException($"Enum value '{item}' does not have a name.");
        }
        var field = type.GetField(name);
        if (field == null)
        {
            throw new ArgumentException($"Enum value '{item}' does not have a field.");
        }

        if (field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is not DescriptionAttribute attribute)
        {
            return "";
        }
        return attribute.Description;
    }

    public static bool HasSufficientValues(this Recommendation recommendation)
    {
        if (string.IsNullOrEmpty(recommendation?.Title)) return false;
        return !string.IsNullOrEmpty(recommendation.Description) || !string.IsNullOrEmpty(recommendation.Reasoning);
    }
}