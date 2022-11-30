// See https://aka.ms/new-console-template for more information

using AnalyzerNext;
using Combinatorics.Collections;

var set = new int[] { 1, 2, 3, 4 };
var v1 = new Combinations<int>(set, 3);
foreach (var v in v1)
{
    Console.WriteLine(string.Join(",", v));
}

Console.WriteLine("Hello, World!");
var layout = new LayoutData();
var data = new PocoDatacontainer();
data.LoadFromFolder("D:\\1\\intelli\\");
//var sampler = new Sampler(data, layout);
//var sm = new SymbolMap();

var gen = new LayoutGenerator(new Sampler(data, layout), layout, data);
gen.GenerateLayout();
var printer = new LayoutPrinter(new SymbolMap());

printer.PrintForTable(gen.GeneratedLayout);
int a =1;