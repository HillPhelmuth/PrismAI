using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PrismAI.Core.Models.CultureConciergeModels;

/// <summary>
/// Represents a recommendation with details such as title, type, description, image, and reasoning.
/// </summary>
public class Recommendation
{
    [Description("A required 1-2 sentance description for how this recommendation fits the `Theme`")]
    public required string ThemeJustification { get; set; }
    /// <summary>
    /// Gets or sets the title of the recommendation.
    /// </summary>
    [Description("The title of the recommendation.")]
    public string? Title { get; set; }
    
    /// <summary>
    /// Gets or sets the type of the recommendation (e.g., movie, book, event).
    /// </summary>
    [Description("The type of the recommendation (e.g., movie, book, event).")]
    public string? Type { get; set; }
    [Description("The qloo entity id most directly related to this recommendation")]
    public required Guid EntityId { get; set; }
    [Description("The qloo entity type most directly related to this recommendation. Must be one of urn:entity:brand, urn:entity:artist, urn:entity:book, urn:entity:destination, urn:entity:person, urn:entity:place, urn:entity:podcast, urn:entity:movie, urn:entity:tv_show,or urn:entity:videogame")]
    public required string EntityTypeId { get; set; }
    /// <summary>
    /// Gets or sets the description of the recommendation.
    /// </summary>
    [Description("A description providing details about the recommendation.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the image URL associated with the recommendation.
    /// </summary>
    [Description("The image URL associated with the recommendation. Should come from either the Qloo results or an image web search")]
    public required string ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the reasoning behind the recommendation.
    /// </summary>
    [Description("The reasoning behind why this recommendation was made. Essentially a summary of Why This Fits. Include specific `affinity` scores")]
    public string? Reasoning { get; set; }
   
    [Description("The order of the recommendation in the timeline, Should correspond to to the value in `TimelineTime`")]
    public int TimelineOrder { get; set; }
    [Description("The estimated time to complete the recommendation in minutes.")]
    public int EstimatedTimeToComplete { get; set; }
}

public class ImageResponse
{
    public required string ImageUrl { get; set; }
    public required string Justification { get; set; }
}
public class EntityTypeResponses
{
    [Description("Step by step justification for the entity type selections and counts")]
    public required string Reasoning { get; set; }
    [Description("A list of entity type responses, each the selected entity type and the number required. If the number is zero, do not add it to array")]
    public required List<EntityTypeResponse> EntityTypeResponseItems { get; set; }
}
public class EntityTypeResponse
{
    
    [Description("The entity type selected")]
    public required EntityType EntityType { get; set; }
    [Description("Number of recommendations to return for this entity type. Must be between 1 and 3")]
    [Range(1, 3)]
    public required int NumberOfRecommendations { get; set; }
}