// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text;
using AnalyzerNext;
using AnalyzerUtils;
using Combinatorics.Collections;
using MoreLinq;
using MoreLinq.Extensions;

//var transformer = new LayoutTransformer();
//transformer.GetQwertyForSkewmak();

var w = new LayoutWeights();

Console.OutputEncoding = Encoding.UTF8;

var source = Sources.RiderEN_Simple;
var layout = new LayoutConfig();
var data = new PocoDatacontainer();
data.LoadFromFolder(source.DataPath, 0.2f);
var iterPath = "d:\\1\\Iterator\\iterData.txt";

var all = data.SymbolMap.LettersLower.SubtractElementWise("arstneiozjqx");
Console.WriteLine(all.Count());
//ShowWorstKeysByMinAdjacency(data); return;

var iterator = new IterativeAutoLayoutGenerator(new LayoutWeights(), layout, data, iterPath);
iterator.Generate();
return;

var kg = new KeySetsGenerator(layout, data);
kg.GenerateLayout();
return;


var gen = new LayoutGenerator(new Sampler(data, layout), layout, data);
gen.GenerateLayout();
var printer = new LayoutPrinter(new SymbolMap(data.LowerLetterLanguage));

printer.PrintForTable(gen.GeneratedLayout);
int a = 1;

void ShowWorstKeysByMinAdjacency(PocoDatacontainer pocoDatacontainer)
{
    var mains = $"arstneio".ToCharArray();
    var rest = pocoDatacontainer.SymbolMap.LettersLower.SubtractElementWise(mains);
    var dict = new Dictionary<char, float>();
    foreach (var ch in rest)
    {
        dict.Add(ch, 0);
        foreach (var m in pocoDatacontainer.SymbolMap.LettersLower)
        {
            var metric = pocoDatacontainer.GetAdjMetric(ch, m);
            dict[ch] += metric;
        }
    }

    var sorted = dict.OrderByDescending(k => k.Value);
    foreach (var kv in sorted)
    {
        Console.WriteLine(kv.Key + $" : " + kv.Value);
    }
}