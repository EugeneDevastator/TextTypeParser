using AnalyzerUtils;

namespace AnalyzerNext;

public class PermutationLookuper
{
    public (CharArray l, float score) FindLayoutByPermutation(
        ISampler sampler,
        CharArray baseLayout, 
        char[] charsToPlace,
        IEnumerable<List<KeyCoord>> permutations)
    {
        var bestLayout = new CharArray(baseLayout);
        var scanLayout = new CharArray(baseLayout);
        var minScore = sampler.Sample(baseLayout);
        Console.WriteLine($"current score:{minScore}");

        foreach (var coords in permutations)
        {
            //fill layout
            for (var i = 0; i < coords.Count; i++)
            {
                scanLayout[coords[i]] = charsToPlace[i];
            }
            
            var score = sampler.Sample(scanLayout);

            if (score < minScore)
            {
                bestLayout = new CharArray(scanLayout);
                minScore = score;
            }
        }

        return (bestLayout, minScore);
    }
    
}