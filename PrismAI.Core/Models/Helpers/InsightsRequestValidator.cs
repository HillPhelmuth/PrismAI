// InsightsRequest validation helper
// This class validates an InsightsRequest instance against the
// “Entity Type Parameter Guide” at https://docs.qloo.com/reference/available-parameters-by-entity-type
// Album params: :contentReference[oaicite:0]{index=0}
// Artist params: :contentReference[oaicite:1]{index=1}
// Book params: :contentReference[oaicite:2]{index=2}
// Brand params: :contentReference[oaicite:3]{index=3}
// Destination params: :contentReference[oaicite:4]{index=4}
// Movie params: :contentReference[oaicite:5]{index=5}
// Person params: :contentReference[oaicite:6]{index=6}
// Place params: :contentReference[oaicite:7]{index=7}
// Podcast params: :contentReference[oaicite:8]{index=8}
// TV-Show params: :contentReference[oaicite:9]{index=9}
// Video-Game params: :contentReference[oaicite:10]{index=10}

using System.Reflection;
using System.Text.Json.Serialization;

namespace PrismAI.Core.Models.Helpers;

public static class InsightsRequestValidator
{
    // Expose entry-point
    public static bool TryValidate(InsightsRequest request, out List<string> errors)
    {
        errors = [];

        if (request?.Filter is null)
        {
            errors.Add("`filter` object is required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Filter.Type))
        {
            errors.Add("`filter.type` is required.");
            return false;
        }
        if (!request.Filter.Type.StartsWith("urn:entity:", StringComparison.OrdinalIgnoreCase))
        {
            
            return true;
        }
        var type = request.Filter.Type.Trim().ToLowerInvariant();
        if (!Rules.TryGetValue(type, out var rule))
        {
            errors.Add($"Unsupported `filter.type` value: '{type}'.");
            return false;
        }

        // 1. Required parameters
        var present = EnumerateParameterPaths(request);
        errors.AddRange(rule.Required.Where(req => !present.Contains(req))
            .Select(req => $"Parameter '{req}' is required for '{type}' requests."));

        // 2. Illegal parameters
        errors.AddRange(present.Where(p => !rule.Allowed.Contains(p))
            .Select(p => $"Parameter '{p}' is not allowed for '{type}' requests."));

        return errors.Count == 0;
    }

    // ---------- internal plumbing ----------

    private record EntityTypeRules(HashSet<string> Allowed, HashSet<string> Required);

    // Mapping from `filter.type` → allowed/required parameter paths
    private static readonly Dictionary<string, EntityTypeRules> Rules = new(StringComparer.OrdinalIgnoreCase)
    {
        // Album :contentReference[oaicite:11]{index=11}
        ["urn:entity:album"] = new EntityTypeRules(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter.type","filter.exclude.entities","filter.parents.types",
                "filter.popularity.min","filter.popularity.max",
                "filter.release_date.min","filter.release_date.max",
                "filter.results.entities","filter.results.entities.query",
                "filter.tags","operator.filter.tags","filter.exclude.tags","operator.exclude.tags",
                "offset","signal.interests.entities","signal.interests.tags","take"
            },
            ["filter.type"]),

        // Artist :contentReference[oaicite:12]{index=12}
        ["urn:entity:artist"] = new EntityTypeRules(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter.type","bias.trends","filter.exclude.entities","filter.parents.types",
                "filter.popularity.min","filter.popularity.max","filter.exclude.tags","operator.exclude.tags",
                "filter.external.exists","operator.filter.external.exists",
                "filter.results.entities","filter.results.entities.query","filter.tags","operator.filter.tags",
                "offset","signal.demographics.age","signal.demographics.audiences",
                "signal.demographics.audiences.weight","signal.demographics.gender",
                "signal.interests.entities","signal.interests.tags","take"
            },
            ["filter.type"]),

        // Book :contentReference[oaicite:13]{index=13}
        ["urn:entity:book"] = new EntityTypeRules(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter.type","bias.trends","filter.exclude.entities","filter.exclude.tags","operator.exclude.tags",
                "filter.external.exists","operator.filter.external.exists","filter.parents.types",
                "filter.popularity.min","filter.popularity.max","filter.publication_year.min","filter.publication_year.max",
                "filter.results.entities","filter.results.entities.query","filter.tags","operator.filter.tags","offset",
                "signal.demographics.audiences","signal.demographics.age","signal.demographics.audiences.weight",
                "signal.demographics.gender","signal.interests.entities","signal.interests.tags","take"
            },
            ["filter.type"]),

        // Brand :contentReference[oaicite:14]{index=14}
        ["urn:entity:brand"] = new EntityTypeRules(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter.type","bias.trends","filter.exclude.entities","operator.exclude.tags","filter.exclude.tags",
                "filter.external.exists","operator.filter.external.exists","filter.parents.types","filter.popularity.min",
                "filter.popularity.max","filter.results.entities","filter.results.entities.query","filter.tags",
                "operator.filter.tags","signal.demographics.age","signal.demographics.audiences",
                "signal.demographics.audiences.weight","signal.interests.entities","signal.demographics.gender",
                "signal.interests.tags","offset","take"
            },
            ["filter.type"]),

        // Destination (note the extra required param) :contentReference[oaicite:15]{index=15}
        ["urn:entity:destination"] = new EntityTypeRules(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter.type","bias.trends","filter.exclude.entities","filter.external.exists",
                "operator.filter.external.exists","filter.exclude.tags","operator.exclude.tags",
                "filter.geocode.name","filter.geocode.admin1_region","filter.geocode.admin2_region",
                "filter.geocode.country_code","filter.location","filter.location.radius","filter.location.geohash",
                "filter.exclude.location.geohash","filter.parents.types","filter.popularity.min","filter.popularity.max",
                "filter.results.entities","filter.results.entities.query","filter.tags","operator.filter.tags","offset",
                "signal.demographics.age","signal.demographics.audiences","signal.demographics.audiences.weight",
                "signal.demographics.gender","signal.interests.entities","signal.interests.tags","take"
            },
            ["filter.type", "signal.interests.entities"]),

        // Movie :contentReference[oaicite:16]{index=16}
        ["urn:entity:movie"] = new EntityTypeRules(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter.type","bias.trends","filter.content_rating","filter.exclude.entities","filter.external.exists",
                "operator.filter.external.exists","filter.exclude.tags","operator.exclude.tags","filter.parents.types",
                "filter.popularity.min","filter.popularity.max","filter.release_year.min","filter.release_year.max",
                "filter.release_country","operator.filter.release_country","filter.rating.min","filter.rating.max",
                "filter.results.entities","filter.results.entities.query","filter.tags","operator.filter.tags","offset",
                "signal.demographics.audiences","signal.demographics.age","signal.demographics.audiences.weight",
                "signal.demographics.gender","signal.interests.entities","signal.interests.tags","take"
            },
            ["filter.type"]),

        // Person :contentReference[oaicite:17]{index=17}
        ["urn:entity:person"] = new EntityTypeRules(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter.type","bias.trends","filter.date_of_birth.min","filter.date_of_birth.max",
                "filter.date_of_death.min","filter.date_of_death.max","filter.exclude.entities","filter.external.exists",
                "operator.filter.external.exists","filter.exclude.tags","operator.exclude.tags","filter.gender",
                "filter.parents.types","filter.popularity.min","filter.popularity.max","filter.results.entities",
                "filter.results.entities.query","filter.tags","operator.filter.tags","offset",
                "signal.demographics.age","signal.demographics.audiences","signal.demographics.audiences.weight",
                "signal.demographics.gender","signal.interests.entities","signal.interests.tags","take"
            },
            ["filter.type"]),

        // Place :contentReference[oaicite:18]{index=18}
        ["urn:entity:place"] = new EntityTypeRules(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter.type","bias.trends","filter.address","filter.exclude.entities","filter.exclude.tags",
                "operator.exclude.tags","filter.external.exists","operator.filter.external.exists",
                "filter.external.tripadvisor.rating.count.max","filter.external.tripadvisor.rating.count.min",
                "filter.external.tripadvisor.rating.max","filter.external.tripadvisor.rating.min","filter.geocode.name",
                "filter.geocode.admin1_region","filter.geocode.admin2_region","filter.geocode.country_code",
                "filter.hotel_class.max","filter.hotel_class.min","filter.hours","filter.location","filter.location.geohash",
                "filter.exclude.location.geohash","filter.location.radius","filter.parents.types","filter.popularity.min",
                "filter.popularity.max","filter.price_level.min","filter.price_level.max","filter.price_range.from",
                "filter.price_range.to","filter.properties.business_rating.min","filter.properties.business_rating.max",
                "filter.properties.resy.rating.min","filter.properties.resy.rating.max","filter.references_brand",
                "filter.results.entities","filter.results.entities.query","filter.resy.rating_count.min",
                "filter.resy.rating_count.max","filter.resy.rating.party.min","filter.resy.rating.party.max",
                "filter.tags","operator.filter.tags","offset","signal.demographics.age","signal.demographics.audiences",
                "signal.demographics.audiences.weight","signal.demographics.gender","signal.interests.entities",
                "signal.interests.tags","take"
            },
            ["filter.type"]),

        // Podcast :contentReference[oaicite:19]{index=19}
        ["urn:entity:podcast"] = new EntityTypeRules(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter.type","bias.trends","filter.exclude.entities","filter.exclude.tags","operator.exclude.tags",
                "filter.external.exists","operator.filter.external.exists","filter.parents.types","filter.popularity.max",
                "filter.popularity.min","filter.results.entities","filter.results.entities.query","filter.tags",
                "operator.filter.tags","offset","signal.demographics.gender","signal.demographics.age",
                "signal.demographics.audiences","signal.demographics.audiences.weight","signal.interests.entities",
                "signal.interests.tags","take"
            },
            ["filter.type"]),

        // TV Show :contentReference[oaicite:20]{index=20}
        ["urn:entity:tv_show"] = new EntityTypeRules(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter.type","bias.trends","filter.content_rating","filter.exclude.entities","filter.external.exists",
                "operator.filter.external.exists","filter.exclude.tags","operator.exclude.tags","filter.finale_year.max",
                "filter.finale_year.min","filter.latest_known_year.max","filter.latest_known_year.min","filter.parents.types",
                "filter.popularity.max","filter.popularity.min","filter.release_year.max","filter.release_year.min",
                "filter.release_country","operator.filter.release_country","filter.rating.max","filter.rating.min",
                "filter.results.entities","filter.results.entities.query","filter.tags","operator.filter.tags","offset",
                "signal.demographics.age","signal.demographics.audiences","signal.demographics.audiences.weight",
                "signal.demographics.gender","signal.interests.entities","signal.interests.tags","take"
            },
            ["filter.type"]),

        // Video Game :contentReference[oaicite:21]{index=21}
        ["urn:entity:video_game"] = new EntityTypeRules(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter.type","bias.trends","filter.exclude.entities","filter.exclude.tags","operator.exclude.tags",
                "filter.external.exists","operator.filter.external.exists","filter.parents.types","filter.popularity.min",
                "filter.popularity.max","filter.results.entities","filter.results.entities.query","filter.tags",
                "operator.filter.tags","offset","signal.demographics.age","signal.demographics.audiences",
                "signal.demographics.audiences.weight","signal.demographics.gender","signal.interests.entities",
                "signal.interests.tags","take"
            },
            ["filter.type"])
    };

    // Recursively enumerate parameter paths that are non-null in the request
    private static HashSet<string> EnumerateParameterPaths(object? obj, string? prefix = null)
    {
        HashSet<string> paths = new(StringComparer.OrdinalIgnoreCase);
        if (obj == null) return paths;

        var t = obj.GetType();
        if (t == typeof(string) || t.IsPrimitive || obj is DateTime) return paths;

        foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = prop.GetValue(obj);
            if (value == null) continue;

            var attr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
            var name = attr?.Name ?? prop.Name;
            var full = string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";

            if (IsLeaf(prop.PropertyType))
            {
                paths.Add(full);
            }
            else
            {
                // include container property path too (e.g. "filter.release_year")
                paths.Add(full);
                foreach (var sub in EnumerateParameterPaths(value, full))
                    paths.Add(sub);
            }
        }

        return paths;
    }

    private static bool IsLeaf(Type t) =>
        t.IsPrimitive || t == typeof(string) || typeof(System.Collections.IEnumerable).IsAssignableFrom(t) && t != typeof(string);

    public static HeatmapResult ToHeatmapResult(this InsightsResults results)
    {
        // Convert InsightsResults.Heatmap (List<HeatmapPoint>) to HeatmapResult (List<HeatmapLocation>)
        var heatmapLocations = new List<HeatmapLocation>();
        if (results?.Heatmap != null)
        {
            foreach (var point in results.Heatmap)
            {
                var location = new Location
                {
                    Latitude = point.Location.Latitude,
                    Longitude = point.Location.Longitude,
                    Geohash = point.Location.Geohash
                };
                var metrics = new Metrics
                {
                    Affinity = point.Query.Affinity,
                    AffinityRank = point.Query.AffinityRank,
                    Popularity = point.Query.Popularity
                };
                heatmapLocations.Add(new HeatmapLocation
                {
                    Location = location,
                    Query = metrics
                });
            }
        }
        return new HeatmapResult { Results = heatmapLocations };
    }
}