﻿// See https://aka.ms/new-console-template for more information

using System;

//Console.WriteLine("Hello, World!");
//var parser = new Parser();
//parser.Parse();

var data = new PocoDatacontainer();
var parser = new ParserNext(data);
parser.Parse();