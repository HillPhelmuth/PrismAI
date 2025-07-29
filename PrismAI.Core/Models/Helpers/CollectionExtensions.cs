namespace PrismAI.Core.Models.Helpers;

public static class CollectionExtensions
{
    public static List<T> ShuffleList<T>(this List<T> list)
    {
        if (list.Count == 0) return list;
        var rng = new Random();
        var n = list.Count;
        while (n > 1)
        {
            var k = rng.Next(n--);
            (list[n], list[k]) = (list[k], list[n]); // Swap elements
        }
        return list;
    }
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        return list.ShuffleList();
    }
}