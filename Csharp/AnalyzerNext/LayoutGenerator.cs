using Combinatorics.Collections;

namespace AnalyzerNext;

public class LayoutGenerator
{
    private Sampler _sampler;
    public char[,] GeneratedLayout;
    private LayoutData _layout;
    private IDataContainer _data;

    public LayoutGenerator(Sampler sampler, LayoutData layout, IDataContainer data)
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
        Copy2D(_layout.FixedKeys, bestCurrentLayout);
        
        foreach (var posSet in posSets)
        {
            var keysToGet = Math.Min(posSet.Count, keysRemain.Length);
            if (keysToGet == 0)
                //end of keys and must return layout asis
                break;
            
            var sampleKeys = keysRemain[..keysToGet];
            sampleKeys = FillToCount(sampleKeys, posSet.Count);
            Console.WriteLine(sampleKeys);
            keysRemain = Expel(keysRemain, sampleKeys);

            //var permutations = new Variations<char>(sampleKeys.ToCharArray(), posSet.Count);
            var permutations = new Permutations<char>(sampleKeys.ToCharArray());
            Console.WriteLine(permutations.Count);

            var sampleMinScore = float.MaxValue;
            Copy2D(bestCurrentLayout, testLayout);
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
                    Copy2D(testLayout, bestCurrentLayout);
                    sampleMinScore = score;
                }
            }
            
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
    private string Expel(string source, params string[] exiled)
    {
        var exile = string.Join("", exiled);
        return new string(source.Where(k => !exile.Contains(k)).ToArray());
    }

    private string FillToCount(string source, int count)
    {
        var missing = count - source.Length;
        if (missing < 1)
            return source;
        
        source += new string(LayoutData.SKIP, missing);
        return source;
    }

    public void Copy2D(char[,] source, char[,] dst)
    {
        for (int i = 0; i < source.GetLength(0); i++)
        {
            for (int k = 0; k < source.GetLength(1); k++)
            {
                dst[i, k] = source[i, k];
            }
        }
    }
}