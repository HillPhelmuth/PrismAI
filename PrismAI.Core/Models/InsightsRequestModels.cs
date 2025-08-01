using System.ComponentModel;
using System.Text.Json.Serialization;
using PrismAI.Core.Models.Attributes;
using PrismAI.Core.Models.Helpers;

namespace PrismAI.Core.Models;
//[TypeConverter(typeof(Helpers.GenericTypeConverter<InsightsRequestModels>))]
public class InsightsRequestModels
{
    [Description("Request properties for an Qloo Insights api")]
    public InsightsRequest InsightsRequest { get; set; } = new();
}


public class InsightsRequest
{
    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter parameters that constrain the query.")]
    public FilterParams? Filter { get; set; }

    [JsonPropertyName("signal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Audience or context signals that influence affinity scoring.")]
    public SignalParams? Signal { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Options controlling pagination, sorting, and explainability.")]
    public OutputParams? Output { get; set; }
    [JsonPropertyName("feature.explainability")]
    [Description("When set to true, the response includes explainability metadata for each recommendation and for the overall result set.")]
    public bool Explainability { get; set; }
}
[TypeConverter(typeof(GenericTypeConverter<FilterParams>))]
public class FilterParams
{

    [JsonPropertyName("type")]
    [Description("Filter by the entity type to return (e.g. `urn:entity:place`) or as demographic filter (`urn:demographics`) or taste analysis (`urn:tag`).")]
    public string Type => SetType();

    [JsonIgnore]
    [Description("Type of data you want to filter for")]
    public FilterType FilterType { get; set; }

    [JsonIgnore]
    [Description("Entity type to filter by. Value will be ignored unless `FilterType` is `Entities`")]
    public EntityType EntityType { get; set; }
    [JsonPropertyName("address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by address using a partial string query.")]
    public string? Address { get; set; }

    [JsonPropertyName("audience.types")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a list of audience types.")]
    public List<string>? AudienceTypes { get; set; }

    [JsonPropertyName("content_rating")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a comma-separated list of content ratings based on the MPAA film rating system, which determines suitability for various audiences.")]
    public string? ContentRating { get; set; }

    [JsonPropertyName("date_of_birth.min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Earliest birth date, inclusive (YYYY-MM-DD).")]
    public string? DateOfBirthMin { get; set; }

    [JsonPropertyName("date_of_birth.max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Latest birth date, inclusive (YYYY-MM-DD).")]
    public string? DateOfBirthMax { get; set; }

    [JsonPropertyName("date_of_death.min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Earliest death date, inclusive (YYYY-MM-DD).")]
    public string? DateOfDeathMin { get; set; }

    [JsonPropertyName("date_of_death.max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Latest death date, inclusive (YYYY-MM-DD).")]
    public string? DateOfDeathMax { get; set; }

    [JsonPropertyName("results.entities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a comma-separated list of entity IDs. Often used to assess the affinity of an entity towards input.")]
    public string? Entities { get; set; }

    [JsonPropertyName("exclude.entities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Entity URNs to exclude.")]
    public string? ExcludeEntities { get; set; }

    [JsonPropertyName("exclude.tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Tag URNs to exclude.")]
    public string? ExcludeTags { get; set; }

    [JsonPropertyName("exclude.location.query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Place name to exclude.")]
    public string? ExcludeLocationQuery { get; set; }

    [JsonPropertyName("exclude.location.radius")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Radius in meters for exclusion boundary.")]
    public int? ExcludeLocationRadius { get; set; }

    [JsonPropertyName("gender")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter results to align with a specific gender identity. Used to personalize output based on known or inferred gender preferences.")]
    public string? Gender { get; set; }


    [JsonPropertyName("geocode.country_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by properties.geocode.country_code. Exact match (two-letter country code).")]
    public string? GeocodeCountryCode { get; set; }

    //[JsonPropertyName("geocode.name")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //[Description("Filter by properties.geocode.name. Exact match (usually city or town name).")]
    //public string? GeocodeName { get; set; }


    [JsonPropertyName("hours")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by the day of the week the Point of Interest must be open (Monday, Tuesday, etc.).")]
    public string? Hours { get; set; }

    [JsonPropertyName("ids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a comma-separated list of audience IDs.")]
    public string? Ids { get; set; }

    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a WKT POINT, POLYGON, MULTIPOLYGON or a single Qloo ID for a named urn:entity:locality. WKT is formatted as X then Y, therefore longitude is first (POINT(-73.99823 40.722668)). If a Qloo ID or WKT POLYGON is passed, location.radius will create a fuzzy boundary when set to a value > 0.")]
    public string? Location { get; set; }

    [JsonPropertyName("location.query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("A query used to search for one or more named locality Qloo IDs for filtering requests. Always use this for location filter if no WKT POINT is available")]
    public string? LocationQuery { get; set; }

    [JsonPropertyName("location.geohash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a geohash. Geohashes are generated using the Python package pygeohash with a precision of 12 characters. This parameter returns all POIs that start with the specified geohash. For example, supplying dr5rs would allow returning the geohash dr5rsjk4sr2w.")]
    public string? LocationGeohash { get; set; }

    [JsonPropertyName("location.radius")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by the radius (in meters) when also supplying location or location.query.")]
    public int? LocationRadius { get; set; }

    [JsonPropertyName("parents.types")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a comma seperated list of parental entity types (urn:entity:place). Find parent entity types with `GetTagTypes`.")]
    public string? ParentsTypes { get; set; }

    [JsonPropertyName("popularity.min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by the minimum popularity percentile required for a Point of Interest (float, between 0 and 1; closer to 1 indicates higher popularity, e.g., 0.98 for the 98th percentile).")]
    public float? PopularityMin { get; set; }

    [JsonPropertyName("popularity.max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by the maximum popularity percentile a Point of Interest must have (float, between 0 and 1; closer to 1 indicates higher popularity, e.g., 0.98 for the 98th percentile).")]
    public float? PopularityMax { get; set; }

    [JsonPropertyName("price_level.min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by the minimum price level a Point of Interest can have (1|2|3|4, similar to dollar signs).")]
    public int? PriceLevelMin { get; set; }

    [JsonPropertyName("price_level.max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by the maximum price level a Point of Interest can have (1|2|3|4, similar to dollar signs).")]
    public int? PriceLevelMax { get; set; }

    [JsonPropertyName("price_range.from")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter places by a minimum price level, representing the lowest price in the desired range. Accepts an integer value between 0 and 1,000,000.")]
    public int? PriceRangeFrom { get; set; }

    [JsonPropertyName("price_range.to")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter places by a maximum price level, representing the highest price in the desired range. Accepts an integer value between 0 and 1,000,000.")]
    public int? PriceRangeTo { get; set; }

    [JsonPropertyName("rating.min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by the minimum Qloo rating a Point of Interest must havtag.typese (float, between 0 and 5).")]
    public float? RatingMin { get; set; }

    [JsonPropertyName("rating.max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by the maximum Qloo rating a Point of Interest must have (float, between 0 and 5).")]
    public float? RatingMax { get; set; }

    [JsonPropertyName("references_brand")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a list of brand entity IDs. Use this to narrow down place recommendations to specific brands. For example, to include only Walmart stores, pass the Walmart brand ID. Each ID must match exactly.")]
    public List<string>? ReferencesBrand { get; set; }

    [JsonPropertyName("tag.types")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a list of comma seperated child tag types. You can retrieve a complete list of parent and child tag types via `GetTagTypes`.")]
    public string? TagTypes { get; set; }

    [JsonPropertyName("tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a comma-separated list of tag IDs (urn:tag:genre:restaurant:Italian).")]
    public string? Tags { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Specifies how multiple filter.tags values are combined in the query. Use \"union\" (equivalent to a logical \"or\") to return results that match at least one of the specified tags, or \"intersection\" (equivalent to a logical \"and\") to return only results that match all specified tags. The default is \"union\".")]
    [QueryStringPart("operator.filter.tags")]
    public string? FilterTagsOperator { get; set; }

    [JsonPropertyName("release_country")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Filter by a list of countries where a movie or TV show was originally released.")]
    public List<string>? ReleaseCountry { get; set; }
    public string SetType()
    {
        return FilterType != FilterType.Entities ? FilterType.GetDescriptionAttribute() : $"{FilterType.GetDescriptionAttribute()}{EntityType.GetDescriptionAttribute()}";
    }
}

[TypeConverter(typeof(GenericTypeConverter<SignalParams>))]
public class SignalParams
{
    // Demographics signals - flattened
    [JsonPropertyName("demographics.audiences")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Audience segment URNs.")]
    public List<string>? DemographicsAudiences { get; set; }

    [JsonPropertyName("demographics.audiences.weight")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Weight of audience influence.")]
    public string? DemographicsAudiencesWeight { get; set; }

    [JsonPropertyName("demographics.age")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Age cohort identifiers (options are `35_and_younger`, `36_to_55`, and `55_and_older`).")]
    public string? DemographicsAge { get; set; }
    [JsonPropertyName("demographics.age.weight")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Weighting of age signal influence (options are `very_low`,`low`,`mid`,`high`, and `very_high`).")]
    public string? DemographicsAgeWeight { get; set; }

    [JsonPropertyName("demographics.gender")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Gender identifier (male or female).")]
    public string? DemographicsGender { get; set; }
    [JsonPropertyName("demographics.gender.weight")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Weighting of gender signal influence")]
    public string? DemographicsGenderWeight { get; set; }

    // Interest signals - flattened
    [JsonPropertyName("interests.entities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Comma separated list of valid entity ids (entity ids are in UUID format)")]
    public string? InterestsEntities { get; set; }

    [JsonPropertyName("interests.tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Comma seperated tag interests that **must** each start with 'urn:' (e.g. `urn:tag:genre:media:horror,urn:tag:genre:media:thriller`)")]
    public string? InterestsTags { get; set; }

    [JsonPropertyName("interests.tags.operator")]
    [QueryStringPart("operator.signal.interests.tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Specifies how multiple signal.interests.tags values are combined in the query.\r\n\r\nUse \"union\" (equivalent to a logical \"or\") to return results that contain at least one of the specified tags. In this mode, the tag with the highest affinity is used for scoring. - Use \"intersection\" (equivalent to a logical \"and\") or leave this field empty to return results that contain all specified tags, with affinity scores merged across them.")]
    public string? InterestsTagsOperator { get; set; }

    // Location signal - flattened
    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Locality URN or WKT `POINT` representing the location.")]
    public string? Location { get; set; }

    [JsonPropertyName("location.radius")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Radius in meters around the location.")]
    public int? LocationRadius { get; set; }

    [JsonPropertyName("trends")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The level of impact a trending entity has on the results. Must be one of 'low', 'medium', 'high' or 'off'")]
    [QueryStringPart("bias.trends")]
    public string? Trends { get; set; }
}

[TypeConverter(typeof(GenericTypeConverter<OutputParams>))]
public class OutputParams
{
    [JsonPropertyName("take")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Maximum number of results to return.")]
    public int? Take { get; set; }

    [JsonPropertyName("page")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Page number for paginated results.")]
    public int? Page { get; set; }

    [JsonPropertyName("offset")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Number of results to skip.")]
    public int? Offset { get; set; }

    [JsonPropertyName("sort_by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Primary sort field (affinity or distance).")]
    public string? SortBy { get; set; }

    [JsonPropertyName("diversify.by")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Property used to diversify results (e.g., properties.geocode.city).")]
    public string? DiversifyBy { get; set; }

    [JsonPropertyName("diversify.take")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Maximum results per diversify group.")]
    public int? DiversifyTake { get; set; }

    [JsonPropertyName("feature.explainability")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Whether to include explainability data.")]
    public bool? Explainability { get; set; }

    [JsonPropertyName("output.heatmap.boundary")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Spatial boundary granularity for heatmap results (geohash, city, neighborhood).")]
    public string? HeatmapBoundary { get; set; }
}

