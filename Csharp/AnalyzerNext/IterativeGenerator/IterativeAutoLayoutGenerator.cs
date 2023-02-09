using AnalyzerUtils;
using Combinatorics.Collections;
using MoreLinq;
using MoreLinq.Extensions;
using static AnalyzerUtils.Constants;

namespace AnalyzerNext;

// note: to find which keys to dupe do layout parsing with dropput and without minor keys (which should not be duped anyway)

public class IterativeAutoLayoutGenerator
{
    private CharMatrix _initialLayout;
    private CharMatrix _toFillLayout;
    private LayoutConfig _config;
    private LayoutWeights _weights;
    private IDataContainer _data;
    private List<KeyCoord> fillCoords = new List<KeyCoord>();
    private char[] _nonCharacters;
    private char[] _skip;
    private string[] _lines;
    private string _layoutFileName;
    private int _replacementsMax;
    private string _fixedChars;

    public IterativeAutoLayoutGenerator(LayoutWeights weights,
        LayoutConfig config, 
        IDataContainer data,
        string layoutFileName,
        int replacementsMax,
        string fixedChars)
    {
        _fixedChars = fixedChars;
        _replacementsMax = replacementsMax;
        _layoutFileName = layoutFileName;
        _data = data;
        _weights = weights;
        _config = config;
        _lines = File.ReadAllLines(layoutFileName);
        _initialLayout = new CharMatrix(_lines[..3]);
        _nonCharacters = _config.SkippedKeys.ToCharArray();
        _skip = new char[] { IGNORE, TOFILL, EMPTY };
    }

    public void Generate()
    {
        var printer = new LayoutPrinter(_data.SymbolMap);
        var sampler = new CachedSampler(_weights, _data);
        _toFillLayout = new CharMatrix(_initialLayout);
        _toFillLayout.ReplaceAllWithOne(EMPTY, TOFILL);
        _toFillLayout.ReplaceAllWithOne(_nonCharacters, IGNORE);


        //return;
        var missing = _data.SymbolMap.DistinctLowerKeys.SubtractElementWise(_toFillLayout.Flatten);
        var worst = sampler
            .GetNWorstKeys(_replacementsMax, _initialLayout, _fixedChars, "_*^", _data.SymbolMap.DistinctLowerKeys)
            .ToArray();
        var toplace = missing.Concat(worst).ToList();
        var realLimit = Math.Min(toplace.Count, _replacementsMax);
        var keysToInsert = toplace.Take(realLimit).ToList();

        //worst = _data.CountPerKey.
        //    Where(k=>missing.Contains(k.Key))
        //    .OrderByDescending(k => k.Value)
        //    .Take(6)
        //    .Select(k => k.Key).ToArray();
        //Console.WriteLine(worst);

        _toFillLayout.ReplaceAllWithOne(keysToInsert.ToArray(), TOFILL);

        Console.WriteLine(_toFillLayout);
        sampler.CacheLayoutNoSkipsOnly(_toFillLayout, IGNORE);
        var layoutDupes = _toFillLayout.Flatten.Distinct().Where(f => _toFillLayout.Flatten.Count(k => k == f) > 1)
            .ToArray();
        sampler.ListOrderedWeights(_initialLayout,
            _data.SymbolMap.DistinctLowerKeys.SubtractElementWise(layoutDupes));
        var fillCoords = _toFillLayout.CoordsIterator.Where(c => _toFillLayout[c] == TOFILL).ToArray();

        var diff = fillCoords.Length - keysToInsert.Count;
        if (diff > 0)
        {
            keysToInsert.AddRange(new string(EMPTY, diff).ToCharArray());
        }

        var bestLayout = new CharMatrix(_initialLayout);
        var scanLayout = new CharMatrix(_initialLayout);
        var skipsAndDupes = layoutDupes.Concat(_skip).Distinct().ToArray();
        var samplingWhiteList = _data.SymbolMap.DistinctLowerKeys.SubtractElementWise(skipsAndDupes).ToArray();

        var minScore = sampler.Sample(bestLayout, ref samplingWhiteList);

        Console.WriteLine($"current score:{minScore}");

        var charsToSkip = new Combinations<char>(toplace, 0, GenerateOption.WithoutRepetition);
        foreach (var dropout in charsToSkip)
        {
            var dropSample = keysToInsert.SubtractElementWise(dropout).ToList();
            dropSample.AddRange(new string(EMPTY, dropout.Count));
            var perms = new Permutations<char>(dropSample);
            Console.WriteLine("Perm Count " + perms.Count);

            object Locker = "";

            perms.AsParallel().Select(p =>
                {
                    var sl = new CharMatrix(_initialLayout);
                    foreach (var c in p.Zip(fillCoords))
                    {
                        sl[c.Second.x, c.Second.y] = c.First;
                    }

                    var score = sampler.Sample(sl, ref samplingWhiteList);
                    return (sl, score);
                })
                .ForAll(r =>
                {
                    if (r.score < minScore)
                    {
                        lock (Locker)
                        {
                            if (r.score < minScore)
                            {
                                minScore = r.score;
                                bestLayout.CopyDataFrom(r.sl);
                                Console.WriteLine(minScore);
                            }
                        }
                    }
                });

            //foreach (var perm in perms.AsParallel())
            //{
            //    
            //    //fill layout
            //    foreach (var c in perm.Zip(fillCoords))
            //    {
            //        scanLayout[c.Second.x, c.Second.y] = c.First;
            //    }
//
            //    //var score = sampler.Sample(scanLayout, ref allDupes, ref skipsAndDupes);
            //    var score = sampler.Sample(scanLayout, ref samplingWhiteList);
//
            //    if (score < minScore)
            //    {
            //        bestLayout.CopyDataFrom(scanLayout);
            //        minScore = score;
            //        Console.WriteLine(minScore);
            //    }
            //}
        }

        foreach (var c in bestLayout.CoordsIterator)
        {
            if (bestLayout[c] == IGNORE && _initialLayout[c] != bestLayout[c])
                bestLayout[c] = _initialLayout[c];
        }

        printer.PrintForTable(bestLayout);
        Console.WriteLine($"new score:{minScore}");

        //File.WriteAllText();
        var l = bestLayout.ToStringList();
        l.Add("");
        l.AddRange(_lines);
        File.WriteAllLines(_layoutFileName, l);
    }
}