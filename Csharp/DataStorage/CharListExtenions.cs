namespace AnalyzerNext;

public static class CharListExtenions
{
    public static IEnumerable<(char a, char b)> CrossProduct(this IEnumerable<char> self, IEnumerable<char> other)
    {
        foreach (var c in self)
        {
            foreach (var o in other)
            {
                yield return (c, o);
            }
        }
    }

    public static IEnumerable<char> SubtractElementWise(this IEnumerable<char> self, IEnumerable<char> other)
    {
        return self.Where(s => !other.Contains(s));
    }
}