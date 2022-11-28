using System;
using System.Net.Security;
using CharData;
using Combinatorics.Collections;
using MainApp;
using Microsoft.Win32.SafeHandles;
using NumSharp;


var analyzer = new Analyzer();
analyzer.GenerateLayout();
//TODO try to split layout into two sets of keys that are farther away from each other,
//then do same  spreads inside each subset.
//test perms.
var a = "abc".ToCharArray();
var idx = a.Select((c, i) => i).ToArray();
var prm = new Permutations<int>(idx,GenerateOption.WithoutRepetition);
foreach (var p in prm)
{
    Console.WriteLine(p.Select(v => a[v]).ToArray());
}