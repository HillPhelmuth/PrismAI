using System.Text.Json.Serialization;

namespace PrismAI.Core.Models;

public sealed record DemographicsChartDto
{
    /// <summary>Display order for the age categories.</summary>
    [JsonPropertyName("ageBuckets")]
    public List<string> AgeBuckets { get; set; } = new();

    /// <summary>One block per entity/tag returned from Qloo.</summary>
    [JsonPropertyName("entities")]
    public List<EntityBlock> Entities { get; set; } = new();

    public sealed record EntityBlock
    {
        /// <summary>Friendly name (e.g. “technology”).</summary>
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        /// <summary>Affinity values in the same order as <see cref="DemographicsChartDto.AgeBuckets"/>.</summary>
        [JsonPropertyName("24_and_younger")]
        public double TwentyFourAndYounger { get; set; }
        [JsonPropertyName("25_to_29")]
        public double TwentyFiveToTwentyNine { get; set; }
        [JsonPropertyName("30_to_34")]
        public double ThirtyToThirtyFour { get; set; }
        [JsonPropertyName("35_to_44")]
        public double ThirtyFiveToFortyFour { get; set; }
        [JsonPropertyName("45_to_54")]
        public double FortyFiveToFiftyFour { get; set; }
        [JsonPropertyName("55_and_older")]
        public double FiftyFiveAndOlder { get; set; }

        [JsonPropertyName("male")] public double Male { get; set; }
        [JsonPropertyName("female")] public double Female { get; set; }
    }

    /* ------------------------------------------------------------------
       Helper: Build from InsightsResponse.Results.Demographics
       ------------------------------------------------------------------ */
    public static DemographicsChartDto FromQlooDemographics(InsightsResults qlooDemo)
    {
        // Pull out the canonical bucket order once (you can also hard-code it)
        var buckets = new List<string> {
            "24_and_younger","25_to_29","30_to_34",
            "35_to_44","45_to_54","55_and_older"
        };

        return new DemographicsChartDto
        {
            AgeBuckets = buckets,
            Entities = qlooDemo.Demographics
                .Select(d => new EntityBlock
                {
                    Label = d.EntityId.ToString().Split(':').Last(),
                    TwentyFourAndYounger = d.Query.Age.Age24AndYounger.GetValueOrDefault(),
                    TwentyFiveToTwentyNine = d.Query.Age.Age25To29.GetValueOrDefault(),
                    ThirtyToThirtyFour = d.Query.Age.Age30To34.GetValueOrDefault(),
                    ThirtyFiveToFortyFour = d.Query.Age.Age35To44.GetValueOrDefault(),
                    FortyFiveToFiftyFour = d.Query.Age.Age45To54.GetValueOrDefault(),
                    FiftyFiveAndOlder = d.Query.Age.Age55AndOlder.GetValueOrDefault(),
                    Male = (double)d.Query.Gender.Male!,
                    Female = (double)d.Query.Gender.Female!
                })
                .ToList()
        };
    }
    
}