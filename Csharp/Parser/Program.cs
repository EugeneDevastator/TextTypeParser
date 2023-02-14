// See https://aka.ms/new-console-template for more information

using System;
using System.Text;

//Console.WriteLine("Hello, World!");
//var parser = new Parser();
//parser.Parse();
Console.OutputEncoding = Encoding.UTF8;

var data = new PocoDatacontainer();
var parser = new EnumerableParser(data, Sources.RiderEN);
parser.Parse();