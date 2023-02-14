using AnalyzerUtils;
using MoreLinq;
using MoreLinq.Extensions;
using NumSharp;

namespace AnalyzerNext;

public class CachedSampler
{
    private readonly List<(KeyCoord a, KeyCoord b, float w)> _meteringCoordsCached = new();
    private readonly List<(KeyCoord a, KeyCoord b, float w)> _meteringCoordsAll = new();
    private readonly IDataContainer _data;
    private LayoutWeights _weights;

    public CachedSampler(LayoutWeights weights, IDataContainer data)
    {
        _weights = weights;
        _data = data;
        
        foreach (var kv in weights.WeightedTargets)
        {
            _meteringCoordsAll.AddRange(
                weights.WeightedTargets[kv.Key]
                    .Select(target => (kv.Key, target, target.w)));
        }
    }

    public void CacheLayouttoFill(CharMatrix emptyPlacesTemplate, char placeSymbol)
    {
        foreach (var (x, y) in emptyPlacesTemplate.CoordsList)
        {
            if (!_weights.WeightedTargets.ContainsKey((x, y)))
                continue;

            if (emptyPlacesTemplate[x, y] == placeSymbol)
            {
                _meteringCoordsCached.AddRange(
                    _weights.WeightedTargets[(x, y)]
                        .Select(target => (new KeyCoord(x, y), target, target.w)));
            }
        }
    }
    public void CacheLayoutNoSkipsOnly(CharMatrix unfilledTemplate, char skipSymbol)
    {
        foreach (var (x, y) in unfilledTemplate.CoordsList)
        {
            if (!_weights.WeightedTargets.ContainsKey((x, y)))
                continue;

            if (unfilledTemplate[x, y] != skipSymbol)
            {
                _meteringCoordsCached.AddRange(
                    _weights.WeightedTargets[(x, y)]
                        .Select(target => (new KeyCoord(x, y), target, target.w)));
            }
        }
    }
    public List<char> GetNWorstKeys(int n, CharMatrix layout, 
        IEnumerable<char> dontList, 
        IEnumerable<char> dontMeter,
        IEnumerable<char> whiteList)
    {
        var sortedScores = GetKeysSortedByBadness(layout, dontList, dontMeter, whiteList);
        return sortedScores[..n].ToList();

    }
    public List<char> GetNWorstAndBestKeys(int n, CharMatrix layout, IEnumerable<char> dontList, IEnumerable<char> dontMeter, IEnumerable<char> whitelist)
    {
        var sortedScores = GetKeysSortedByBadness(layout, dontList, dontMeter, whitelist);
        var top = sortedScores[..n];
        var bot = sortedScores[^n..];
        return
            top.Concat(bot).ToList();
        //.Take(n)
        //.ToList();
    }
    
    public void ListOrderedWeights(CharMatrix layout, IEnumerable<char> allowed)
    {
        float sum = 0;
        Dictionary<(char c, char c2), float> pairWeights = new();
        foreach (var coords in _meteringCoordsAll)
        {
            var keyA = layout[coords.a];
            var keyB = layout[coords.b];

            if (allowed.Contains(keyA) && allowed.Contains(keyB))
            {
                pairWeights.Add((keyA, keyB), _data.GetAdjMetric(keyA, keyB) * coords.w);
            }
        }

        foreach (var kv in pairWeights.OrderByDescending(kv=>kv.Value))
        {
            Console.WriteLine($"{kv.Key.c},{kv.Key.c2} => {kv.Value}");
        }
    }
    
    private char[] GetKeysSortedByBadness(CharMatrix layout, IEnumerable<char> dontList, IEnumerable<char> dontMeter,
        IEnumerable<char> whiteList)
    {
        var nonTypable = new List<char> { Constants.EMPTY, Constants.IGNORE, Constants.TOFILL };
        var keysUsed = layout.Flatten;
        var duplicated = keysUsed.Distinct().Where(k => keysUsed.Count(scan => scan == k) > 1);
        var skipForMetering = nonTypable.Concat(duplicated).Concat(dontMeter);
        var skipForCandidates = nonTypable.Concat(duplicated).Concat(dontList);

        var keyScores = _data.SymbolMap.DistinctLowerKeys.ToDictionary(l => l, l => 0f);

        foreach (var coords in _meteringCoordsAll)
        {
            var keyA = layout[coords.a];
            var keyB = layout[coords.b];
            if (!whiteList.Contains(keyA) || !whiteList.Contains(keyB))
                continue;

            keyScores[keyA] += _data.GetAdjMetric(keyA, keyB) * coords.w;
        }

        var sortedScores = keyScores
            .Where(kv => !skipForCandidates.Contains(kv.Key))
            .OrderByDescending(k => k.Value)
            .Select(k => k.Key)
            .ToArray();
        return sortedScores;
    }

    public float Sample(CharMatrix layout, ref char[] allowed)
    {
        float sum = 0;
        foreach (var coords in _meteringCoordsCached)
        {
            var keyA = layout[coords.a];
            var keyB = layout[coords.b];

            if(allowed.Contains(keyA) && allowed.Contains(keyB))
                sum += _data.GetAdjMetric(keyA, keyB) * coords.w;
        }

        return sum;
    }

    public float SampleAll(CharMatrix layout, ref char[] allowed)
    {
        float sum = 0;
        foreach (var coords in _meteringCoordsAll)
        {
            var keyA = layout[coords.a];
            var keyB = layout[coords.b];

            if(allowed.Contains(keyA) && allowed.Contains(keyB))
                sum += _data.GetAdjMetric(keyA, keyB) * coords.w;
        }

        return sum;
    }
}