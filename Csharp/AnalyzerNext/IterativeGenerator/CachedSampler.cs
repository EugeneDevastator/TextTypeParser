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

    public void CacheLayouttoFill(CharArray emptyPlacesTemplate, char placeSymbol)
    {
        foreach (var (x, y) in emptyPlacesTemplate.CoordsIterator)
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

    public List<char> GetNWorstKeys(int n, CharArray layout, IEnumerable<char> dontList, IEnumerable<char> dontMeter)
    {
        var sortedScores = GetKEysSortedByBadness(layout, dontList, dontMeter);
        return sortedScores[..n].ToList();

    }
    public List<char> GetNWorstAndBestKeys(int n, CharArray layout, IEnumerable<char> dontList, IEnumerable<char> dontMeter)
    {
        var sortedScores = GetKEysSortedByBadness(layout, dontList, dontMeter);
        var top = sortedScores[..n];
        var bot = sortedScores[^n..];
        return
            top.Concat(bot).ToList();
        //.Take(n)
        //.ToList();
    }
    private char[] GetKEysSortedByBadness(CharArray layout, IEnumerable<char> dontList, IEnumerable<char> dontMeter)
    {
        var nonTypable = new List<char> { Constants.EMPTY, Constants.IGNORE, Constants.TOFILL };
        var keysUsed = layout.Flatten;
        var duplicated = keysUsed.Distinct().Where(k => keysUsed.Count(scan => scan == k) > 1);
        var skipForMetering = nonTypable.Concat(duplicated).Concat(dontMeter);
        var skipForCandidates = nonTypable.Concat(duplicated).Concat(dontList);

        var keyScores = _data.SymbolMap.LettersLower.ToDictionary(l => l, l => 0f);

        foreach (var coords in _meteringCoordsAll)
        {
            var keyA = layout[coords.a];
            var keyB = layout[coords.b];
            if (skipForMetering.Contains(keyA) || skipForMetering.Contains(keyB))
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

    public float Sample(CharArray layout, ref char[] dupes, ref char[] skips)
    {
        float sum = 0;
        foreach (var coords in _meteringCoordsCached)
        {
            var keyA = layout[coords.a];
            var keyB = layout[coords.b];

            if (skips.Any(k => k == keyA || k == keyB))
                continue;

            sum += _data.GetAdjMetric(keyA, keyB) * coords.w;
        }

        return sum;
    }

    public float SampleAll(CharArray layout, char[] allowed)
    {
        float sum = 0;
        foreach (var coords in _meteringCoordsAll)
        {
            var keyA = layout[coords.a];
            var keyB = layout[coords.b];

            if (allowed.Any(k => k == keyA) && allowed.Any(k => k == keyB))
                sum += _data.GetAdjMetric(keyA, keyB) * coords.w;
        }

        return sum;
    }
}