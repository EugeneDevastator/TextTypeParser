using AnalyzerUtils;

namespace AnalyzerNext;

public class PermutationLookuper
{
    public (CharMatrix l, float score) FindLayoutByPermutation(
        ISampler sampler,
        CharMatrix baseLayout, 
        char[] charsToPlace,
        IEnumerable<List<KeyCoord>> permutations)
    {
        var bestLayout = new CharMatrix(baseLayout);
        var scanLayout = new CharMatrix(baseLayout);
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
                bestLayout = new CharMatrix(scanLayout);
                minScore = score;
            }
        }

        return (bestLayout, minScore);
    }
    
}