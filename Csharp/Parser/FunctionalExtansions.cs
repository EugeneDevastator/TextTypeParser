public static class FunctionalExtansions
{
    public static IEnumerable<O> Apply<O, I>(this I s, Func<I, IEnumerable<O>> func)
    {
        return func(s);
    }
}