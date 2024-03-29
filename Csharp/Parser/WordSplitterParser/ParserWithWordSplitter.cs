﻿using System.Security.Principal;
using System.Text;
using MoreLinq;
using NumSharp;

[Obsolete]
public class ParserWithWordSplitter
{
    
    /// TODO:
    /// add additional data per leter.
    /// f(char,int) = count of letter at word pos;
    /// f(char,int) = count of intellisense pos e.x.: BackEnd = 01230123
    /// weight calculation bust be normalized per set? w=/wtotal.
    //private string[] typableNames => _keyData.typableNames;

    private int[,] adjZero;
    private int[,] adjOne;
    private int[] keyCounts;

    private string keys;
    private SymbolMap _symbols;
    private WordSplitter _worder;
    private IDataContainer _data;
    private ParseParams _parseParams;

    public ParserWithWordSplitter(IDataContainer data, ParseParams parseParams)
    {
        _parseParams = parseParams;
        _data = data;
        _symbols = new SymbolMap(parseParams.Languages);
        keys = _symbols.DistinctLowerKeys;
        _worder = new WordSplitter(_symbols);
        Console.WriteLine(_symbols.DistinctLowerKeys.ToString());

        //_keyData = new KeyData(_symbolMap.UniqueKeys);

        keyCounts = new int[keys.Length];
        adjZero = new int[keys.Length, keys.Length];
        adjOne = new int[keys.Length, keys.Length];

        for (int i = 0; i < keys.Length; i++)
        {
            keyCounts[i] = 0;
        }
    }

    private bool IsValidVis(char v) => _symbols.AllVisualSymbols.Contains(v);
    private int IndexOfVis(char v) => _symbols.VisToIndex(v);

    
    public void Parse()
    {
        DirectoryInfo d = new DirectoryInfo(_parseParams.ParsingPath); //Assuming Test is your Folder
        FileInfo[] Files = d.GetFiles("*"); //Getting Text files
        var text = GetAllData(Files);
         //GetCrumbs(text,99);
        if (_parseParams.Flags.HasFlag(WorderFlags.IntelliSense))
            ExtractCrumbData(text,3);
        if (_parseParams.Flags.HasFlag(WorderFlags.SimpleText))
            ExtractDataAllChars(text);
        if (_parseParams.Flags.HasFlag(WorderFlags.First4Letters))
            ExtractDataAllCharsFirstNOfWord(text, 4);
        WriteDataFiles();
    }

    private void WriteDataFiles()
    {
        //TODO delegate to datasource
        _data.Fill(keyCounts,adjZero,adjOne, _symbols);
        _data.SaveToFolder(_parseParams.DataPath);
    }

    private void ExtractDataAllCharsFirstNOfWord(string content, int firstn)
    {
        StringBuilder sampleOut = new();
        int curr = 0;
        char keyOfVisual;
        string separ = " ;.\"" + '\n';

        {
            var UPPER = _symbols.AllProjectedLowerKeys.ToUpper();
            var Letters = _symbols.AllProjectedLowerKeys.ToUpper() + _symbols.AllProjectedLowerKeys;
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
                if (_symbols.AllVisualSymbols.Contains(last))
                {
                    AddCountData(_symbols.VisToIndex(word[^1]));
                    if (sampleOut.Length < 3000)
                        sampleOut.Append(word[^1]);
                }
            }

            void LogSequence(string seq)
            {
                ResetWord();
                foreach (var cr in seq)
                {
                    if (_symbols.AllVisualSymbols.Contains(cr))
                    {
                        keyOfVisual = _symbols.VisualToKey(cr);
                        kc = kb;
                        kb = ka;
                        ka = keyOfVisual;

                        ic = ib;
                        ib = ia;
                        ia = _symbols.KeyToIndex(keyOfVisual);
                        //cba
                        if (kc != '\0')
                        {
                            adjOne[ic, ia] += 1;
                           // Console.Write("1:" + kc.ToString()+ka.ToString()+" ");
                        }

                        if (kb != '\0')
                        {
                            adjZero[ib, ia] += 1;
                            //Console.Write("0:" + kb.ToString() + ka.ToString() + " ");
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
        //File.WriteAllText(Path.Combine(Constants.rootPath,"All.txt"),result);
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
                            if (IsValidVis(pair.a))
                                AddCountData(pair.a);
                            break;
                        case 0:
                            if (IsValidVis(pair.a) && IsValidVis(pair.b))
                                AddAdjacencyZero(pair.a,pair.b);
                            break;
                        case 1:
                            if (IsValidVis(pair.a) && IsValidVis(pair.b))
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

                if (_symbols.AllVisualSymbols.Contains(cr))
                {
                    key = _symbols.VisualToKey(cr);
                    kc = kb;
                    kb = ka;
                    ka = key;

                    ic = ib;
                    ib = ia;
                    ia = _symbols.KeyToIndex(key);
                    
                    AddCountData(_symbols.KeyToIndex(key));
                    //Fir: a = F
                    //b=F
                    //a=i
                    //c=F, b=i, a=r
                    if (kc != '\0')
                    {
                        adjOne[ic, ia] += 1;
                        //Console.Write("1:" + kc.ToString()+ka.ToString()+" ");
                    }

                    if (kb != '\0')
                    {
                        adjZero[ib, ia] += 1;
                        //Console.Write("0:" + kb.ToString() + ka.ToString() + " ");
                    }
                    if (_symbols.WordSeparators.Contains(cr))
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
        adjOne[IndexOfVis(first), IndexOfVis(next)] += 1;
    }
    
    private void AddAdjacencyZero(char first, char next)
    {
        //so turns out numpy sucks at setting values..
        adjZero[IndexOfVis(first), IndexOfVis(next)] += 1;
    }
    
    private void AddAdjacencyData(byte ione, byte itwo, byte itri)
    {
        //so turns out numpy sucks at setting values..
        adjOne[ione, itri] += 1;
        adjZero[itwo, itri] += 1;
    }

    private void AddCountData(char c)
    {
        keyCounts[IndexOfVis(c)] += 1;
    }
    
    private void AddCountData(byte idx)
    {
        keyCounts[idx] += 1;
    }
}