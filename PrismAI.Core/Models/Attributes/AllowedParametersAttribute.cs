namespace PrismAI.Core.Models.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class AllowedParametersAttribute : Attribute
{
    private const string CommonParameters =
        "filter.type,bias.trends,filter.exclude.entities,filter.exclude.tags,operator.exclude.tags,filter.external.exists,operator.filter.external.exists,filter.parents.types,filter.popularity.min,filter.popularity.max,filter.results.entities,filter.results.entities.query,filter.tags,operator.filter.tags,offset,signal.demographics.age,signal.demographics.audiences,signal.demographics.audiences.weight,signal.demographics.gender,signal.interests.entities,signal.interests.tags,take";

    /// <summary>
    /// Comma-separated list of allowed entity type parameters (including all common parameters).
    /// </summary>
    public string EntityTypeParameter { get; }

    public AllowedParametersAttribute(string allowedParameters)
    {
        if (string.IsNullOrWhiteSpace(allowedParameters))
        {
            EntityTypeParameter = CommonParameters;
        }
        else
        {
            EntityTypeParameter = allowedParameters + "," + CommonParameters;
        }
    }
}