using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CharData;
using MainApp;
using NumSharp;

public class Parser
{
    
    /// TODO:
    /// add additional data per leter.
    /// f(char,int) = count of letter at word pos;
    /// f(char,int) = count of intellisense pos e.x.: BackEnd = 01230123
    /// weight calculation bust be normalized per set? w=/wtotal.

    private char[] typableChars => _keyData.typable.ToCharArray();
    private string[] typableNames => _keyData.typableNames;

    private float[,] adjFloatZero;
    private float[,] adjFloatOne;
    private float[,] adjFloatOneBegins;
    private long[] typableCounts;

    private NDArray adjacencyZero;
    private NDArray adjacencyOne;

    private NDArray adjacencyOneBegginings;

    //private float[,] adjacency;
    //private Dictionary<char, int> typableCounts = new Dictionary<char, int>();
    private IReadOnlyDictionary<char, byte> typIndices => _keyData.TypIndices;
    private string typable => _keyData.typable;
    private KeyData _keyData;
    private SymbolMap _symbolMap;
    private WordSplitter _worder;

    public Parser()
    {
        _symbolMap = new SymbolMap();
        _worder = new WordSplitter(_symbolMap);
        Console.WriteLine(_symbolMap.UniqueKeys.ToString());

        _keyData = new KeyData(_symbolMap.UniqueKeys);

        adjacencyZero = np.zeros((typable.Length, typable.Length), NPTypeCode.Float);
        adjacencyOne = np.zeros((typable.Length, typable.Length), NPTypeCode.Float);
        adjacencyOneBegginings = np.zeros((typable.Length, typable.Length), NPTypeCode.Float);
        typableCounts = new long[typable.Length];
        adjFloatZero = new float[typable.Length, typable.Length];
        adjFloatOne = new float[typable.Length, typable.Length];
        adjFloatOneBegins = new float[typable.Length, typable.Length];


        for (int i = 0; i < typable.Length; i++)
        {
            typableCounts[i] = 0;
        }
    }

    private bool IsValid(char c) => _symbolMap.VisualSymbols.Contains(c);
    private int IndexOf(char c) => typIndices[_symbolMap.VisualToKey(c)];
    
    public void Parse()
    {
        DirectoryInfo d = new DirectoryInfo(Constants.parsePath); //Assuming Test is your Folder
        FileInfo[] Files = d.GetFiles("*"); //Getting Text files
        var text = GetAllData(Files);
        //GetCrumbs(text,99);
        ExtractCrumbData(text,3);
        //ExtractDataAllChars(text);
        //ExtractDataAllCharsFirstNOfWord(text, 4);
        WriteDataFiles();
        WriteTables();
        WriteCounts();
    }

    private void WriteTables()
    {
        WriteCounts();
        WriteAdjTable(adjacencyZero, "AdjZeroDir.csv", "ADJ ZERO DIR");
        WriteAdjTable(adjacencyZero+adjacencyZero.transpose(), "AdjZeroAny.csv", "ADJ ZERO any");
        WriteAdjTable(adjacencyOne, "AdjOneDir.csv", "ADJ ONE DIR");
        WriteAdjTable(adjacencyOne+adjacencyOne.transpose(), "AdjOneDir.csv", "ADJ ONE any");
    }
    
    private void WriteDataFiles()
    {
        for (int i = 0; i < typable.Length; i++)
        {
            for (int k = 0; k < typable.Length; k++)
            {
                adjacencyZero[i, k] = adjFloatZero[i, k];
                adjacencyOne[i, k] = adjFloatOne[i, k];
            }
        }

        np.save(Path.Combine(Constants.rootPath, Constants.AdjZeroDatafile), adjacencyZero);
        np.save(Path.Combine(Constants.rootPath, Constants.AdjOneDatafile), adjacencyOne);
        np.save(Path.Combine(Constants.rootPath, Constants.CountsDatafile), np.asarray(typableCounts));
        File.WriteAllText(Path.Combine(Constants.rootPath, Constants.KeySetData), typable);
    }
    private void WriteAdjacencyAny()
    {
        WriteAdjTable(adjacencyZero + adjacencyZero.transpose(),  Constants.adjZeroAnyName, "sdf");
        // StringBuilder output = new StringBuilder();
        // output.Append("BIDIR ADJ;");
        // for (int k = 0; k < typable.Length; k++)
        //     output.Append(typableNames[k]).Append(";");
        // output.Append("\n");
//
        // for (int k = 0; k < typable.Length; k++)
        // {
        //     output.Append(typableNames[k]).Append(";");
        //     for (var i = 0; i < typable.Length; i++)
        //     {
        //         output.Append((adjacencyOne[i, k]+adjacencyOne[k, i]).ToString()).Append(";");
        //     }
//
        //     output.Append("\n");
        // }
//
        // File.WriteAllText(Path.Combine(Constants.rootPath, Constants.adjRawName), output.ToString());
    }

    private void WriteSortedAdjacency()
    {
        //WriteAdj(adjacencyZero,Constants.sortedDirectAdjName)
      // StringBuilder output = new StringBuilder();
      // output.Append("SORT DIR ADJ;");

      // for (int k = 0; k < typable.Length; k++)
      // {
      //     var idx = adjacencyZero[k].argsort<float>();
      //     var sVals = adjacencyZero[k][idx];

      //     output.Append(typableNames[k]).Append(";");
      //     for (int i = 0; i < typable.Length; i++)
      //         output.Append(typableNames[idx[i]]).Append(";");
      //     output.Append('\n');

      //     output.Append(typableNames[k]).Append(";");
      //     for (int i = 0; i < typable.Length; i++)
      //         output.Append(sVals[i].ToString()).Append(";");
      //     output.Append('\n');
      // }

      // File.WriteAllText(Path.Combine(Constants.rootPath, Constants.sortedDirectAdjName), output.ToString());
    }

    private void WriteAdjTable(NDArray table, string fname, string header)
    {
        StringBuilder output = new StringBuilder();
        output.Append(header+";");
        for (int k = 0; k < typable.Length; k++)
            output.Append(typableNames[k]).Append(";");
        output.Append("\n");

        for (int k = 0; k < typable.Length; k++)
        {
            output.Append(typableNames[k]).Append(";");
            for (var i = 0; i < typable.Length; i++)
            {
                output.Append((table[i, k]).ToString()).Append(";");
            }
            output.Append("\n");
        }

        File.WriteAllText(Path.Combine(Constants.rootPath, fname), output.ToString());
    }

    private void WriteSortedAdjacencyAny()
    {
        StringBuilder output = new StringBuilder();
        output.Append("SORT bidir ADJ;");

        for (int k = 0; k < typable.Length; k++)
        {
            //sorted indices and vals
            var sIdx = adjacencyOne[k].argsort<float>();
            var sVals = adjacencyOne[k][sIdx];

            output.Append(typableNames[k]).Append(";");
            for (int i = 0; i < typable.Length; i++)
                output.Append(typableNames[sIdx[i]]).Append(";");
            output.Append('\n');

            output.Append(typableNames[k]).Append(";");
            for (int i = 0; i < typable.Length; i++)
                output.Append(sVals[i].ToString()).Append(";");
            output.Append('\n');
        }

        File.WriteAllText(Path.Combine(Constants.rootPath, Constants.sortedBiDirAdjName), output.ToString());
    }

    private void WriteCounts()
    {
        StringBuilder output = new StringBuilder();

        for (int k = 0; k < typable.Length; k++)
        {
            output.Append(typableNames[k]).Append(";").Append(typableCounts[k]).Append("\n");
        }

        File.WriteAllText(Path.Combine(Constants.rootPath, Constants.countsName), output.ToString());
    }

    private void ExtractDataAllCharsFirstNOfWord(string content, int firstn)
    {
        StringBuilder sampleOut = new();
        int curr = 0;
        char keyOfSym;
        string separ = " ;.\"" + '\n';

        {
            var UPPER = _symbolMap.LowerLetters.ToUpper();
            var Letters = _symbolMap.LowerLetters.ToUpper() + _symbolMap.LowerLetters;
            int pos = 0;
            char ka = '\0', kb = '\0', kc = '\0';
            byte ia = 0, ib = 0, ic = 0;

            byte posInWord = 0;

            StringBuilder wordBuilder = new StringBuilder();

            foreach (var cr in content.ToCharArray())
            {
                wordBuilder.Append(cr);
                if (!Letters.Contains(cr))
                {
                    ParseOneWord(wordBuilder.ToString());
                    wordBuilder.Clear();
                }
            }

            void ParseOneWord(string word)
            {
                AddWordTerminator(word);

                //skip empty words for now.
                if (!Letters.Contains(word[0]))
                    return;
                
                //log out first letters
                var maxlast = Math.Min(word.Length, firstn);
                LogSequence(word[..maxlast]);

                //log out sequence of capses.
                StringBuilder s = new StringBuilder();
                foreach (var ch in word)
                {
                    if (UPPER.Contains(ch))
                        s.Append(ch);
                }

                LogSequence(s.ToString());
            }

            void ResetWord()
            {
                if (sampleOut.Length < 3000)
                    sampleOut.Append("|");
                kc = ka = kb = '\0';
            }

            void AddWordTerminator(string word)
            {
                
                var last = word[^1];
                if (_symbolMap.VisualSymbols.Contains(last))
                {
                    AddCountData(typIndices[_symbolMap.VisualToKey(word[^1])]);
                    if (sampleOut.Length < 3000)
                        sampleOut.Append(word[^1]);
                }
            }

            void LogSequence(string seq)
            {
                ResetWord();
                foreach (var cr in seq)
                {
                    if (_symbolMap.VisualSymbols.Contains(cr))
                    {
                        keyOfSym = _symbolMap.VisualToKey(cr);
                        kc = kb;
                        kb = ka;
                        ka = keyOfSym;

                        ic = ib;
                        ib = ia;
                        ia = typIndices[keyOfSym];
                        //cba
                        if (kc != '\0')
                        {
                            adjFloatOne[ic, ia] += 1;
                            Console.Write("1:" + kc.ToString()+ka.ToString()+" ");
                        }

                        if (kb != '\0')
                        {
                            adjFloatZero[ib, ia] += 1;
                            Console.Write("0:" + kb.ToString() + ka.ToString() + " ");
                        }

                        AddCountData(ia);
                        
                        if (sampleOut.Length < 3000)
                            sampleOut.Append(cr);
                    }
                    else // hit the separator.
                    {
                        kc = '\0';
                        kb = '\0';
                        ka = '\0';
                    }
                }
            }
        }
        Console.WriteLine("SAMPLE: " + sampleOut);
    }

    private string GetAllData(FileInfo[] Files)
    {
        float total = Files.Length;
        int curr = 0;
        char c;
        StringBuilder allofthem = new StringBuilder();
        foreach (var f in Files)
        {
            if (curr % 20 == 0)
                Console.WriteLine("progress" + curr / total);
            curr++;

            allofthem.Append(File.ReadAllText(f.FullName));
        }

        var result = allofthem.ToString();
        File.WriteAllText(Path.Combine(Constants.rootPath,"All.txt"),result);
        return result;
        
    }

    private void GetCrumbs(string content, int nchars)
    {
        foreach (var word in _worder.WordsOf(content,true))
        {
            //Console.Write(word +"|");
            foreach (var crumb in _worder.IntelCrumbsOf(word,3))
            {
                foreach (var adj in _worder.AdjOf(crumb))
                {
                    Console.Write(adj+"|");    
                }
            }
        }
    }

    private void ExtractCrumbData(string content, int range)
    {
        foreach (var word in _worder.WordsOf(content,true))
        {
            //Console.Write(word +"|");
            foreach (var crumb in _worder.IntelCrumbsOf(word,3))
            {
                foreach (var pair in _worder.AdjOf(crumb))
                {
                    switch (pair.spread)
                    {
                        case -1:
                            if (IsValid(pair.a))
                                AddCountData(pair.a);
                            break;
                        case 0:
                            if (IsValid(pair.a) && IsValid(pair.b))
                                AddAdjacencyZero(pair.a,pair.b);
                            break;
                        case 1:
                            if (IsValid(pair.a) && IsValid(pair.b))
                                AddAdjacencyOne(pair.a,pair.b);
                            break;
                        default:
                            break;
                    } 
                }
            }
        }
    }
    
    private void ExtractDataAllChars(string content)
    {
        {
            int pos = 0;
            char ka = '\0', kb = '\0', kc = '\0';
            byte ia = 0, ib = 0, ic = 0;
            char key = '\0';
            float totalSize = content.Length;
            foreach (var cr in content.ToCharArray())
            {
                pos++;
                if (pos % 400000 == 0)
                    Console.WriteLine(pos / totalSize);
                //not really interested in uppercasing.

                if (_symbolMap.VisualSymbols.Contains(cr))
                {
                    key = _symbolMap.VisualToKey(cr);
                    kc = kb;
                    kb = ka;
                    ka = key;

                    ic = ib;
                    ib = ia;
                    ia = typIndices[key];
                    
                    AddCountData(typIndices[key]);
                    //Fir: a = F
                    //b=F
                    //a=i
                    //c=F, b=i, a=r
                    if (kc != '\0')
                    {
                        adjFloatOne[ic, ia] += 1;
                        Console.Write("1:" + kc.ToString()+ka.ToString()+" ");
                    }

                    if (kb != '\0')
                    {
                        adjFloatZero[ib, ia] += 1;
                        Console.Write("0:" + kb.ToString() + ka.ToString() + " ");
                    }
                    if (_symbolMap.WordSeparators.Contains(cr))
                    {
                        kc = ka = kb = '\0';
                    }
                }
            }
        }
    }

    private void AddAdjacencyOne(char first, char next)
    {
        //so turns out numpy sucks at setting values..
        adjFloatOne[IndexOf(first), IndexOf(next)] += 1;
    }
    
    private void AddAdjacencyZero(char first, char next)
    {
        //so turns out numpy sucks at setting values..
        adjFloatZero[IndexOf(first), IndexOf(next)] += 1;
    }
    
    private void AddAdjacencyData(byte ione, byte itwo, byte itri)
    {
        //so turns out numpy sucks at setting values..
        adjFloatOne[ione, itri] += 1;
        adjFloatZero[itwo, itri] += 1;
    }

    private void AddCountData(char c)
    {
        typableCounts[IndexOf(c)] += 1;
    }
    
    private void AddCountData(byte idx)
    {
        typableCounts[idx] += 1;
    }
}