namespace PrismAI.Core.Models.Attributes;

public class QueryStringPartAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}