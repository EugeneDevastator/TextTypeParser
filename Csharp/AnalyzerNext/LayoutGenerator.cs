using AnalyzerNext;
using Combinatorics.Collections;
using static AnalyzerUtils.Utils;

namespace AnalyzerUtils;

public class LayoutGenerator
{
    private Sampler _sampler;
    public char[,] GeneratedLayout;
    private LayoutConfig _layout;
    private IDataContainer _data;

    public LayoutGenerator(Sampler sampler, LayoutConfig layout, IDataContainer data)
    {
        _data = data;
        _layout = layout;
        _sampler = sampler;
    }

    public void GenerateLayout()
    {
        var skipKeys = _layout.SkippedKeys;
        var orderedKeys =
            new string(_data.KeyCounts
                .Select((count, i) => (count, i))
                .OrderByDescending(e => e.count)
                .Select(e => _data.Keys[e.i])
                .ToArray());

        var keysToUse = Expel(orderedKeys, skipKeys, _sampler.GetUsedKeys(_layout.FixedKeys));
        var keysRemain = keysToUse;

        var posSets = _sampler.PriorityPositions();
        char[,] testLayout = new char[_layout.XDim,_layout.YDim];
        char[,] bestCurrentLayout = new char[_layout.XDim,_layout.YDim];
        Copy2D(ref _layout.FixedKeys, ref bestCurrentLayout);
        
        foreach (var posSet in posSets)
        {
            var keysToGet = Math.Min(posSet.Count+_layout.AddToSample, keysRemain.Length);
            if (keysToGet == 0)
                //end of keys and must return layout asis
                break;
            
            var sampleKeys = keysRemain[..keysToGet];
            sampleKeys = FillToCount(sampleKeys, posSet.Count);
            Console.WriteLine(sampleKeys);

            var permutations = new Variations<char>(sampleKeys.ToCharArray(), posSet.Count);
            //var permutations = new Permutations<char>(sampleKeys.ToCharArray());
            Console.WriteLine(permutations.Count);

            var sampleMinScore = float.MaxValue;
            IReadOnlyList<char> bestKeySet = null;
            Copy2D(ref bestCurrentLayout, ref testLayout);
            foreach (var perm in permutations)
            {
                for (int i = 0; i < posSet.Count; i++)
                {
                    var pos = posSet[i];
                    testLayout[pos.x, pos.y] = perm[i];
                }
    
                var score = _sampler.GetLayoutScoreTotal(ref testLayout);
                if (score < sampleMinScore)
                {
                    Copy2D(ref testLayout, ref bestCurrentLayout);
                    sampleMinScore = score;
                    bestKeySet = perm;
                }
            }

            string usedKeys = "";
            if (bestKeySet == null)
            {
                usedKeys = sampleKeys[..posSet.Count];
            }
            else
            {
                usedKeys = string.Join("", bestKeySet)[..posSet.Count];
            }
            
            keysRemain = Expel(keysRemain, usedKeys);
            // make variations for available places
            // var v = new Variations(keys, number of places);
            //
            //
        }
        GeneratedLayout = bestCurrentLayout;
    }

    /// <summary>
    /// remove every symbol of exile from source.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="exiled"></param>
    /// <returns></returns>

}