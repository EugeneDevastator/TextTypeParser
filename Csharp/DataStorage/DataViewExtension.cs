using AnalyzerNext;

public static class DataViewExtension
{
    public static void ShowCharsByCount(this IDataContainer data)
    {
        Console.WriteLine("Keys sorted by raw count");
        var sets = data.KeyCounts.Zip(data.Keys);
        foreach (var t in sets.OrderByDescending(s=>s.First))
        {
            Console.WriteLine(t.Second + " : " + t.First);
        }
    }
    
    public static void ShowKeysBySummedAdjacency(this IDataContainer data, string excluding = "")
    {
        Console.WriteLine("Keys sorted by how much adjacencies they have overall");
        var rest = data.SymbolMap.DistinctLowerKeys.SubtractElementWise(excluding);
        var dict = new Dictionary<char, float>();
        foreach (var ch in rest)
        {
            dict.Add(ch, 0);
            foreach (var m in data.SymbolMap.DistinctLowerKeys)
            {
                var metric = data.GetAdjMetric(ch, m);
                dict[ch] += metric;
            }
        }

        var sorted = dict.OrderByDescending(k => k.Value);
        foreach (var kv in sorted)
        {
            Console.WriteLine(kv.Key + $" : " + kv.Value);
        }
    }
    
    
}