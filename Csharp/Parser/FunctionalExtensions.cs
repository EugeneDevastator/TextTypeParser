namespace Parser;

public static class FunctionalExtensions
{
    public static IEnumerable<O> Apply<O, I>(this I s, Func<I, IEnumerable<O>> func)
    {
        return func(s);
    }
}