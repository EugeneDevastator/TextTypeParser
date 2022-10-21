using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MainApp;
using NumSharp;

public class Parser
{
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
    private CharData.KeyData _keyData;
    private SymbolMap _symbolMap;

    public Parser()
    {
        _symbolMap = new SymbolMap();
        Console.WriteLine(_symbolMap.UniqueKeys.ToString());

        _keyData = new CharData.KeyData(_symbolMap.UniqueKeys);

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

    public void Parse()
    {
        DirectoryInfo d = new DirectoryInfo(Constants.parsePath); //Assuming Test is your Folder
        FileInfo[] Files = d.GetFiles("*"); //Getting Text files
        var text = GetAllData(Files);
        ExtractDataAllChars(text);
        ExtractDataAllCharsFirstNOfWord(text, 3);
        WriteDataFiles();
        WriteSortedAdjacency();
        WriteSortedAdjacencyAny();
        WriteAdjacency();
        WriteAdjacencyAny();
        WriteCounts();
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

    private void WriteAdjacency()
    {
        StringBuilder output = new StringBuilder();
        output.Append("DIR ADJ;");
        for (int k = 0; k < typable.Length; k++)
            output.Append(typableNames[k]).Append(";");
        output.Append("\n");

        for (int k = 0; k < typable.Length; k++)
        {
            output.Append(typableNames[k]).Append(";");
            for (var i = 0; i < typable.Length; i++)
            {
                output.Append(adjacencyZero[i, k].ToString()).Append(";");
            }

            output.Append("\n");
        }

        File.WriteAllText(Path.Combine(Constants.rootPath, Constants.adjDirectName), output.ToString());
    }

    private void WriteAdjacencyAny()
    {
        StringBuilder output = new StringBuilder();
        output.Append("BIDIR ADJ;");
        for (int k = 0; k < typable.Length; k++)
            output.Append(typableNames[k]).Append(";");
        output.Append("\n");

        for (int k = 0; k < typable.Length; k++)
        {
            output.Append(typableNames[k]).Append(";");
            for (var i = 0; i < typable.Length; i++)
            {
                output.Append(adjacencyOne[i, k].ToString()).Append(";");
            }

            output.Append("\n");
        }

        File.WriteAllText(Path.Combine(Constants.rootPath, Constants.adjRawName), output.ToString());
    }

    private void WriteSortedAdjacency()
    {
        StringBuilder output = new StringBuilder();
        output.Append("SORT DIR ADJ;");

        for (int k = 0; k < typable.Length; k++)
        {
            var idx = adjacencyZero[k].argsort<float>();
            var sVals = adjacencyZero[k][idx];

            output.Append(typableNames[k]).Append(";");
            for (int i = 0; i < typable.Length; i++)
                output.Append(typableNames[idx[i]]).Append(";");
            output.Append('\n');

            output.Append(typableNames[k]).Append(";");
            for (int i = 0; i < typable.Length; i++)
                output.Append(sVals[i].ToString()).Append(";");
            output.Append('\n');
        }

        File.WriteAllText(Path.Combine(Constants.rootPath, Constants.sortedDirectAdjName), output.ToString());
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
                //AddWordTerminator(word);

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

                        if (kc != '\0')
                            adjFloatOne[ia, ic] += 1;

                        if (kb != '\0')
                            adjFloatZero[ib, ic] += 1;
                                                        
                        AddCountData(ia);
                        
                        if (sampleOut.Length < 3000)
                            sampleOut.Append(cr);
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
        return allofthem.ToString();
    } 
    
    private void ExtractDataAllChars(string content)
    {
        {
            int pos = 0;
            char ka = '\0', kb = '\0', kc = '\0';
            byte ia = 0, ib = 0, ic = 0;
            char c = '\0';
            float totalSize = content.Length;
            foreach (var cr in content.ToCharArray())
            {
                pos++;
                if (pos % 400000 == 0)
                    Console.WriteLine(pos / totalSize);
                //not really interested in uppercasing.

                if (_symbolMap.VisualSymbols.Contains(cr))
                {
                    c = _symbolMap.VisualToKey(cr);
                    kc = kb;
                    kb = ka;
                    ka = c;

                    ic = ib;
                    ib = ia;
                    ia = typIndices[c];

                    if (kc != '\0')
                    {
                        AddAdjacencyData(ia, ib, ic);
                        AddCountData(ic);
                    }
                }
                else
                {
                    kc = ka = kb = '\0';
                }
            }
        }
    }

    private void AddAdjacencyData(byte ia, byte ib, byte ic)
    {
        //so turns out numpy sucks at setting values..
        adjFloatOne[ia, ic] += 1;
        adjFloatZero[ib, ic] += 1;
    }

    private void AddCountData(byte idx)
    {
        typableCounts[idx] += 1;
    }
}