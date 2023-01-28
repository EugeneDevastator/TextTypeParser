using AnalyzerUtils;
using Combinatorics.Collections;
using MoreLinq.Extensions;
using static AnalyzerUtils.Constants;

namespace AnalyzerNext;

public class IterativeLayoutGenerator
{
    private CharArray _initialLayout;
    private CharArray _toFillLayout;
    private LayoutConfig _config;
    private LayoutWeights _weights;
    private IDataContainer _data;
    private List<KeyCoord> fillCoords = new List<KeyCoord>();
    private CharArray _weightingStandardLayout;
    private char[] _nonCharacters;
    private char[] _skip;
    private string[] _lines;
    private string _layoutFileName;

    public IterativeLayoutGenerator(LayoutWeights weights, LayoutConfig config, IDataContainer data, string layoutFileName)
    {
        _layoutFileName = layoutFileName;
        _data = data;
        _weights = weights;
        _config = config;
        _lines = File.ReadAllLines(layoutFileName);
        _initialLayout = new CharArray(_lines[..3]);
        _toFillLayout = new CharArray(_lines[4..7]);
        _weightingStandardLayout = new CharArray(_initialLayout);
        
        _nonCharacters = _config.SkippedKeys.ToCharArray();
        _weightingStandardLayout.ReplaceAllWithOne(_nonCharacters,IGNORE);
        _skip = new char[] { IGNORE, TOFILL, EMPTY };
    }

    public void Generate()
    {
        var printer = new LayoutPrinter(_data.SymbolMap);
        var sampler = new CachedSampler(_weights, _data);
        
        var worst = sampler.GetNWorstKeys(7, _weightingStandardLayout, "arstneio", "_*^").ToArray();
        Console.WriteLine(worst);
        _toFillLayout = new CharArray(_weightingStandardLayout);
        _toFillLayout.ReplaceAllWithOne(worst,TOFILL);
        _toFillLayout.ReplaceAllWithOne(EMPTY,TOFILL);
        Console.WriteLine(_toFillLayout);
        sampler.CacheLayouttoFill(_toFillLayout, TOFILL);


        var keysToInsert = new List<char>();
        foreach (var (x, y) in _initialLayout.CoordsIterator)
        {
            if (_toFillLayout[x, y] == TOFILL)
            {
                fillCoords.Add((x, y));
                keysToInsert.Add(_initialLayout[x, y]);
            }
        }

        var uniqueKeys = keysToInsert
            .Where(k => _data.SymbolMap.LettersLower.Contains(k))
            .Distinct()
            .ToList();

        var scanDupes = uniqueKeys.Where(k => keysToInsert.Count(sc => sc == k) > 1);
        var layoutDupes = _toFillLayout.Flatten.Distinct().Where(f => _toFillLayout.Flatten.Count(k => k == f) > 1);
        var allDupes = scanDupes.Concat(layoutDupes).ToArray();

        _toFillLayout.ReplaceAllWithOne(_nonCharacters,IGNORE);
        
        var diff= fillCoords.Count - keysToInsert.Count;
        if (diff > 0)
        {
           keysToInsert.AddRange(new string(EMPTY,diff).ToCharArray()); 
        }

        var perms = new Permutations<char>(keysToInsert);

        var bestLayout = new CharArray(_initialLayout);
        var scanLayout = new CharArray(_initialLayout);

        var skipsAndDupes = allDupes.Concat(_skip).ToArray();
        var minScore = sampler.Sample(_toFillLayout, ref allDupes, ref skipsAndDupes);
        
        Console.WriteLine($"current score:{sampler.SampleAll(_initialLayout, _data.SymbolMap.LettersLower.ToCharArray())}");
        var allowedToBeWeighted = _data.SymbolMap.LettersLower.Where(c=>!skipsAndDupes.Contains(c)).ToArray();
        
        foreach (var perm in perms)
        {
            //fill layout
            foreach (var c in perm.Zip(fillCoords))
            {
                scanLayout[c.Second.x, c.Second.y] = c.First;
            }

            //var score = sampler.Sample(scanLayout, ref allDupes, ref skipsAndDupes);
            var score = sampler.SampleAll( scanLayout, allowedToBeWeighted);

            if (score < minScore)
            {
                bestLayout = new CharArray(scanLayout);
                minScore = score;
            }
        }
        
        foreach (var c in bestLayout.CoordsIterator)
        {
            if (bestLayout[c] == IGNORE && _initialLayout[c] != bestLayout[c])
                bestLayout[c] = _initialLayout[c];
        }
        
        printer.PrintForTable(bestLayout);
        Console.WriteLine($"new score:{sampler.SampleAll(bestLayout, _data.SymbolMap.LettersLower.ToCharArray())}");
        
        //File.WriteAllText();
        var l = bestLayout.ToStringList();
        l.Add("");
        l.AddRange(_lines);   
        File.WriteAllLines(_layoutFileName,l);
    }
}