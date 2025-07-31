using System.ComponentModel;

namespace PrismAI.Core.Models.PrismAIModels;

/// <summary>
/// Represents a curated cultural experience with themed recommendations across various entity types.
/// </summary>
public class Experience
{
    [Description("A up to 3 word title for the experience")]
    public required string Title { get; set; }
    /// <summary>
    /// The theme of the experience (e.g., 'Parisian Nightlife').
    /// </summary>
    [Description("The theme of the experience (e.g., 'Parisian Nightlife'). Put this in your own words. Do not regurgitate the user input")]
    public required string Theme { get; set; }

    /// <summary>
    /// A textual description of the experience.
    /// </summary>
    [Description("A 4-5 sentance description of the full experience. Use colorful and friendly language")]
    public required string Description { get; set; }
    /// <summary>
    /// A step-by-step description of the Experience timeline
    /// </summary>
    [Description("A step-by-step description of the Experience timeline")]
    public required string TimelineDescription { get; set; }
    
    
    // Three recommendations for each EntityType
    [Description("First recommendation for artist entity type (urn:entity:artist)")]
    public Recommendation? Artist1 { get; set; }
    [Description("Second recommendation for artist entity type (urn:entity:artist)")]
    public Recommendation? Artist2 { get; set; }
    [Description("Third recommendation for artist entity type (urn:entity:artist)")]
    public Recommendation? Artist3 { get; set; }

    [Description("First recommendation for book entity type (urn:entity:book)")]
    public Recommendation? Book1 { get; set; }
    [Description("Second recommendation for book entity type (urn:entity:book)")]
    public Recommendation? Book2 { get; set; }
    [Description("Third recommendation for book entity type (urn:entity:book)")]
    public Recommendation? Book3 { get; set; }

    [Description("First recommendation for brand entity type (urn:entity:brand)")]
    public Recommendation? Brand1 { get; set; }
    [Description("Second recommendation for brand entity type (urn:entity:brand)")]
    public Recommendation? Brand2 { get; set; }
    [Description("Third recommendation for brand entity type (urn:entity:brand)")]
    public Recommendation? Brand3 { get; set; }

    [Description("First recommendation for destination entity type (urn:entity:destination)")]
    public Recommendation? Destination1 { get; set; }
    [Description("Second recommendation for destination entity type (urn:entity:destination)")]
    public Recommendation? Destination2 { get; set; }
    [Description("Third recommendation for destination entity type (urn:entity:destination)")]
    public Recommendation? Destination3 { get; set; }

    [Description("First recommendation for movie entity type (urn:entity:movie)")]
    public Recommendation? Movie1 { get; set; }
    [Description("Second recommendation for movie entity type (urn:entity:movie)")]
    public Recommendation? Movie2 { get; set; }
    [Description("Third recommendation for movie entity type (urn:entity:movie)")]
    public Recommendation? Movie3 { get; set; }

    [Description("First recommendation for person entity type (urn:entity:person)")]
    public Recommendation? Person1 { get; set; }
    [Description("Second recommendation for person entity type (urn:entity:person)")]
    public Recommendation? Person2 { get; set; }
    [Description("Third recommendation for person entity type (urn:entity:person)")]
    public Recommendation? Person3 { get; set; }

    [Description("First recommendation for place entity type (urn:entity:place)")]
    public Recommendation? Place1 { get; set; }
    [Description("Second recommendation for place entity type (urn:entity:place)")]
    public Recommendation? Place2 { get; set; }
    [Description("Third recommendation for place entity type (urn:entity:place)")]
    public Recommendation? Place3 { get; set; }

    [Description("First recommendation for podcast entity type (urn:entity:podcast)")]
    public Recommendation? Podcast1 { get; set; }
    [Description("Second recommendation for podcast entity type (urn:entity:podcast)")]
    public Recommendation? Podcast2 { get; set; }
    [Description("Third recommendation for podcast entity type (urn:entity:podcast)")]
    public Recommendation? Podcast3 { get; set; }

    [Description("First recommendation for tv_show entity type (urn:entity:tv_show)")]
    public Recommendation? TvShow1 { get; set; }
    [Description("Second recommendation for tv_show entity type (urn:entity:tv_show)")]
    public Recommendation? TvShow2 { get; set; }
    [Description("Third recommendation for tv_show entity type (urn:entity:tv_show)")]
    public Recommendation? TvShow3 { get; set; }

    [Description("First recommendation for videogame entity type (urn:entity:videogame)")]
    public Recommendation? VideoGame1 { get; set; }
    [Description("Second recommendation for videogame entity type (urn:entity:videogame)")]
    public Recommendation? VideoGame2 { get; set; }
    [Description("Third recommendation for videogame entity type (urn:entity:videogame)")]
    public Recommendation? VideoGame3 { get; set; }

    public void UpdateRecommendation(RecommendationSlot slotName, Recommendation? recommendation)
    {
        var slot = slotName.ToString();
        // Use reflection to set the property dynamically
        var property = GetType().GetProperty(slot);
        if (property != null && property.CanWrite)
        {
            property.SetValue(this, recommendation);
        }
        else
        {
            throw new ArgumentException($"Invalid slot name: {slotName}");
        }
    }

    public Dictionary<string, Recommendation> PopulatedRecommendationSlots()
    {
        // Use reflection to get all properties of type Recommendation with non-null values
        var recommendations = new Dictionary<string, Recommendation>();
        foreach (var property in GetType().GetProperties())
        {
            if (property.PropertyType == typeof(Recommendation) && property.GetValue(this) is Recommendation rec)
            {
                recommendations[property.Name] = rec;
            }
        }
        return recommendations;
    }
}

