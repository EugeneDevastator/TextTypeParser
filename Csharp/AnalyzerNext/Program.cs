// See https://aka.ms/new-console-template for more information

using AnalyzerNext;

Console.WriteLine("Hello, World!");
var data = new NumpyDataContainer();
var layout = new LayoutData();
var sampler = new Sampler(data, layout);
var ex = new CSVExporter();
var arr = new int[2, 5]
{
    {1,2,3,4,5},{2,2,2,2,2}
};
ex.WriteData(arr,"d:\\arr.txt");
var read = ex.ReadData<int>("d:\\arr.txt");



int a =1;