// See https://aka.ms/new-console-template for more information

using AnalyzerNext;
using Combinatorics.Collections;
    var layout = new LayoutConfig();
    var data = new PocoDatacontainer();
    data.LoadFromFolder("D:\\1\\intelli\\",0.5f);

    var kg = new KeySetsGenerator(layout,data);
    kg.GenerateLayout();
    return;
    
    var gen = new LayoutGenerator(new Sampler(data, layout), layout, data);
    gen.GenerateLayout();
    var printer = new LayoutPrinter(new SymbolMap());

    printer.PrintForTable(gen.GeneratedLayout);
    int a = 1;
