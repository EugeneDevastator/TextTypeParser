using System.Diagnostics;
using AnalyzerUtils;
using Combinatorics.Collections;
using MoreLinq.Extensions;
using static AnalyzerUtils.Constants;

namespace AnalyzerNext;

public class IterativeLayoutGenerator
{
    private CharMatrix _initialLayout;
    private CharMatrix _toFillLayout;
    private LayoutConfig _config;
    private LayoutWeights _weights;
    private IDataContainer _data;
    private List<KeyCoord> fillCoords = new List<KeyCoord>();
    private CharMatrix _weightingStandardLayout;
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
        _initialLayout = new CharMatrix(_lines[..3]);
        _toFillLayout = new CharMatrix(_lines[4..7]);
        _weightingStandardLayout = new CharMatrix(_initialLayout);
        
        _nonCharacters = _config.SkippedKeys.ToCharArray();
        _weightingStandardLayout.ReplaceAllWithOne(_nonCharacters,IGNORE);
        _skip = new char[] { IGNORE, TOFILL, EMPTY };
    }

    public void Generate()
    {
        var printer = new LayoutPrinter(_data.SymbolMap);
        var sampler = new CachedSampler(_weights, _data);
        
        var worst = sampler.GetNWorstKeys(6, _weightingStandardLayout, "arstneio", "_*^", _data.SymbolMap.AllProjectedLowerKeys).ToArray();
        Console.WriteLine(worst);
        _toFillLayout = new CharMatrix(_weightingStandardLayout);
        _toFillLayout.ReplaceAllWithOne(worst,TOFILL);
        _toFillLayout.ReplaceAllWithOne(EMPTY,TOFILL);
        Console.WriteLine(_toFillLayout);
        sampler.CacheLayouttoFill(_toFillLayout, TOFILL);


        var keysToInsert = new List<char>();
        foreach (var (x, y) in _initialLayout.CoordsList)
        {
            if (_toFillLayout[x, y] == TOFILL)
            {
                fillCoords.Add((x, y));
                keysToInsert.Add(_initialLayout[x, y]);
            }
        }

        var uniqueKeys = keysToInsert
            .Where(k => _data.SymbolMap.AllProjectedLowerKeys.Contains(k))
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
        Console.WriteLine("Perm Count "+ perms.Count);
        
        var bestLayout = new CharMatrix(_initialLayout);
        var scanLayout = new CharMatrix(_initialLayout);

        var skipsAndDupes = allDupes.Concat(_skip).ToArray();
        var minScore = sampler.Sample(_toFillLayout, ref skipsAndDupes);

        var lowerLetters = _data.SymbolMap.AllProjectedLowerKeys.ToCharArray();
        Console.WriteLine($"current score:{sampler.SampleAll(_initialLayout, ref lowerLetters )}");
        var allowedToBeWeighted = _data.SymbolMap.AllProjectedLowerKeys.Where(c=>!skipsAndDupes.Contains(c)).ToArray();
        
        foreach (var perm in perms)
        {
            //fill layout
            foreach (var c in perm.Zip(fillCoords))
            {
                scanLayout[c.Second.x, c.Second.y] = c.First;
            }

            //var score = sampler.Sample(scanLayout, ref allDupes, ref skipsAndDupes);
            var score = sampler.Sample(scanLayout, ref skipsAndDupes);

            if (score < minScore)
            {
                bestLayout = new CharMatrix(scanLayout);
                minScore = score;
                Console.WriteLine(minScore);
            }
        }
        
        foreach (var c in bestLayout.CoordsList)
        {
            if (bestLayout[c] == IGNORE && _initialLayout[c] != bestLayout[c])
                bestLayout[c] = _initialLayout[c];
        }
        
        printer.PrintForTable(bestLayout);
        Console.WriteLine($"new score:{sampler.SampleAll(bestLayout, ref lowerLetters)}");
        
        //File.WriteAllText();
        var l = bestLayout.ToStringList();
        l.Add("");
        l.AddRange(_lines);   
        File.WriteAllLines(_layoutFileName,l);
    }
}