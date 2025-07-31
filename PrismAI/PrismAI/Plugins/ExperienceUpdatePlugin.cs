using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using PrismAI.Core.Models.PrismAIModels;

namespace PrismAI.Plugins;

public class ExperienceUpdatePlugin
{
    [KernelFunction, Description("Update the user experience with a new recommendation")]
    public string UpdateUserExperience(Kernel kernel,
        [Description("The experience slot. Setting this to a current populated slot will replace the existing recommendation, otherwise it will add it to the experience.")] RecommendationSlot experienceSlot,
        [Description("The new recommendation to apply to the experience")] Recommendation newRecommendation)
    {
        // Deserialize the current experience and new recommendation
        if (!kernel.Data.TryGetValue("currentExperienceJson", out var currentExperienceJson))
            throw new ArgumentException("Current experience data not found in kernel context.");
        var currentExperience = JsonSerializer.Deserialize<Experience>(currentExperienceJson.ToString());
        if (currentExperience == null || newRecommendation == null)
        {
            throw new ArgumentException("Invalid input data for updating experience.");
        }
        currentExperience.UpdateRecommendation(experienceSlot, newRecommendation);
        Console.WriteLine($"Updated experience slot {experienceSlot}.\nFinal Experience:\n\n{JsonSerializer.Serialize(currentExperience, new JsonSerializerOptions() {WriteIndented = true})}\n\n");
        return JsonSerializer.Serialize(currentExperience);
        // Apply the new recommendation to the current experience

    }
    [KernelFunction, Description("Update the user experience by removing a recommendation")]
    public string RemoveUserRecommendation(Kernel kernel,
        [Description("The recommendation to remove from the experience")] RecommendationSlot experienceSlot)
    {
        if (!kernel.Data.TryGetValue("currentExperienceJson", out var currentExperienceJson))
            throw new ArgumentException("Current experience data not found in kernel context.");
        var currentExperience = JsonSerializer.Deserialize<Experience>(currentExperienceJson!.ToString());
        if (currentExperience == null)
        {
            throw new ArgumentException("Current experience cannot be null.");
        }

        // Remove the recommendation from the specified slot
        currentExperience.UpdateRecommendation(experienceSlot, null);

        // Serialize the updated experience back to JSON
        return JsonSerializer.Serialize(currentExperience);
    }
}