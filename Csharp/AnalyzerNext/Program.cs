// See https://aka.ms/new-console-template for more information

using System.Text;
using AnalyzerNext;
using AnalyzerUtils;
using Combinatorics.Collections;

var transformer = new LayoutTransformer();
transformer.GetQwertyForSkewmak();
return;
    Console.OutputEncoding= Encoding.UTF8;
    var source = Sources.Cyrilic;
    var layout = new LayoutConfig();
    var data = new PocoDatacontainer();
    
    data.LoadFromFolder(source.DataPath,0.5f);

    var kg = new KeySetsGenerator(layout,data);
    kg.GenerateLayout();
    return;
    
    
    var gen = new LayoutGenerator(new Sampler(data, layout), layout, data);
    gen.GenerateLayout();
    var printer = new LayoutPrinter(new SymbolMap(data.LowerLetterLanguage));

    printer.PrintForTable(gen.GeneratedLayout);
    int a = 1;
