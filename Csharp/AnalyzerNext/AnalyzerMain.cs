// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text;
using AnalyzerNext;
using AnalyzerUtils;
using Combinatorics.Collections;
using MoreLinq;
using MoreLinq.Extensions;

Console.OutputEncoding = Encoding.UTF8;
var transformer = new LayoutTransformer();
transformer.PrintShuffeledSkewmakForStdCyr();

//return;
//transformer.GetQwertyForSkewmak();

var w = new LayoutWeights();

Console.OutputEncoding = Encoding.UTF8;

var source = Sources.RiderEN;
var layout = new LayoutConfig();
var data = new PocoDatacontainer();
data.LoadFromFolder(source.DataPath, 0.2f);
var iterPath = Path.Combine(Sources.WorkingFolder, "Iterator\\iterData.txt");

var all = data.SymbolMap.AllProjectedLowerKeys.SubtractElementWise("arstneiozjqx");
Console.WriteLine(all.Count());

//data.ShowCharsByCount();
//data.ShowKeysBySummedAdjacency( "арнтсиео"); return;


var iterator = new IterativeAutoLayoutGenerator(
    new LayoutWeights(),
    layout,
    data,
    iterPath,
    10,

    //"арнтсиео"
    $"arstnieo"

    //"jxwzq"
);

iterator.Generate();
return;

var kg = new KeySetsGenerator(layout, data);
kg.GenerateLayout();
return;
var gen = new LayoutGenerator(new Sampler(data, layout), layout, data);
gen.GenerateLayout();
var printer = new LayoutPrinter(data.SymbolMap);
return;


printer.PrintForTable(gen.GeneratedLayout);
int a = 1;