using System.ComponentModel;
using System.Text.Json.Serialization;
using PrismAI.Core.Models.Attributes;

namespace PrismAI.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter<EntityType>))]
public enum EntityType
{
    None,

    [Description("artist")]
    [AllowedParameters("")]
    Artist,

    [Description("book")]
    [AllowedParameters("filter.publication_year.min,filter.publication_year.max")]
    Book,

    [Description("brand")]
    [AllowedParameters("")]
    Brand,

    [Description("destination")]
    [AllowedParameters("filter.geocode.name,filter.geocode.admin1_region,filter.geocode.admin2_region,filter.geocode.country_code,filter.location,filter.location.radius,filter.location.geohash,filter.exclude.location.geohash")]
    Destination,

    [Description("movie")]
    [AllowedParameters("filter.content_rating,filter.release_year.min,filter.release_year.max,filter.release_country,operator.filter.release_country,filter.rating.min,filter.rating.max")]
    Movie,

    [Description("person")]
    [AllowedParameters("filter.date_of_birth.min,filter.date_of_birth.max,filter.date_of_death.min,filter.date_of_death.max,filter.gender")]
    Person,

    [Description("place")]
    [AllowedParameters("filter.address,filter.exclude.location.geohash,filter.external.tripadvisor.rating.count.min,filter.external.tripadvisor.rating.count.max,filter.external.tripadvisor.rating.min,filter.external.tripadvisor.rating.max,filter.geocode.name,filter.geocode.admin1_region,filter.geocode.admin2_region,filter.geocode.country_code,filter.hotel_class.min,filter.hotel_class.max,filter.hours,filter.location,filter.location.geohash,filter.location.radius,filter.price_level.min,filter.price_level.max,filter.price_range.from,filter.price_range.to,filter.properties.business_rating.min,filter.properties.business_rating.max,filter.references_brand,filter.resy.rating_count.min,filter.resy.rating_count.max,filter.resy.rating.party.min,filter.resy.rating.party.max")]
    Place,

    [Description("podcast")]
    [AllowedParameters("")]
    Podcast,

    [Description("tv_show")]
    [AllowedParameters("filter.content_rating,filter.finale_year.min,filter.finale_year.max,filter.latest_known_year.min,filter.latest_known_year.max,filter.release_year.min,filter.release_year.max,filter.release_country,operator.filter.release_country,filter.rating.min,filter.rating.max")]
    TvShow,

    [Description("videogame")]
    [AllowedParameters("")]
    VideoGame
}
[JsonConverter(typeof(JsonStringEnumConverter<FilterType>))]
public enum FilterType
{
    [Description("urn:demographics")]
    Demographics,
    [Description("urn:heatmap")]
    Heatmap,
    [Description("urn:tag")]
    Tastes,
    [Description("urn:entity:")]
    Entities
    
}