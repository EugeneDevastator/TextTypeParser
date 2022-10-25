using System;
using CharData;
using Combinatorics.Collections;
using MainApp;
using NumSharp;


var analyzer = new Analyzer();
analyzer.GenerateLayout();

//test perms.
var a = "abc".ToCharArray();
var idx = a.Select((c, i) => i).ToArray();
var prm = new Permutations<int>(idx,GenerateOption.WithoutRepetition);
foreach (var p in prm)
{
    Console.WriteLine(p.Select(v => a[v]).ToArray());
}