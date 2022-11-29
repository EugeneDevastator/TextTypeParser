using System.Text;
using MainApp;
using NumSharp;

public class ParserNext
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
    private SymbolMap _symbolMap;
    private WordSplitter _worder;
    private IDataContainer _data;

    public ParserNext(IDataContainer data)
    {
        _data = data;
        _symbolMap = new SymbolMap();
        keys = _symbolMap.KeyboardKeys;
        _worder = new WordSplitter(_symbolMap);
        Console.WriteLine(_symbolMap.KeyboardKeys.ToString());

        //_keyData = new KeyData(_symbolMap.UniqueKeys);

        keyCounts = new int[keys.Length];
        adjZero = new int[keys.Length, keys.Length];
        adjOne = new int[keys.Length, keys.Length];

        for (int i = 0; i < keys.Length; i++)
        {
            keyCounts[i] = 0;
        }
    }

    private bool IsValidVis(char v) => _symbolMap.SignsVisual.Contains(v);
    private int IndexOfVis(char v) => _symbolMap.VisToIndex(v);
    
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
    }

    private void WriteDataFiles()
    {
        //TODO delegate to datasource
        _data.Fill(keyCounts,adjZero,adjOne);
        _data.SaveToFolder("D:\\1\\intelli\\");
        //  for (int i = 0; i < typable.Length; i++)
        //  {
        //      for (int k = 0; k < typable.Length; k++)
        //      {
        //          adjacencyZero[i, k] = adjFloatZero[i, k];
        //          adjacencyOne[i, k] = adjFloatOne[i, k];
        //      }
        //  }
//
        //  np.save(Path.Combine(Constants.rootPath, Constants.AdjZeroDatafile), adjacencyZero);
        //  np.save(Path.Combine(Constants.rootPath, Constants.AdjOneDatafile), adjacencyOne);
        //  np.save(Path.Combine(Constants.rootPath, Constants.CountsDatafile), np.asarray(typableCounts));
        //  File.WriteAllText(Path.Combine(Constants.rootPath, Constants.KeySetData), typable);
    }

    private void ExtractDataAllCharsFirstNOfWord(string content, int firstn)
    {
        StringBuilder sampleOut = new();
        int curr = 0;
        char keyOfVisual;
        string separ = " ;.\"" + '\n';

        {
            var UPPER = _symbolMap.LettersLower.ToUpper();
            var Letters = _symbolMap.LettersLower.ToUpper() + _symbolMap.LettersLower;
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
                if (_symbolMap.SignsVisual.Contains(last))
                {
                    AddCountData(_symbolMap.VisToIndex(word[^1]));
                    if (sampleOut.Length < 3000)
                        sampleOut.Append(word[^1]);
                }
            }

            void LogSequence(string seq)
            {
                ResetWord();
                foreach (var cr in seq)
                {
                    if (_symbolMap.SignsVisual.Contains(cr))
                    {
                        keyOfVisual = _symbolMap.VisualToKey(cr);
                        kc = kb;
                        kb = ka;
                        ka = keyOfVisual;

                        ic = ib;
                        ib = ia;
                        ia = _symbolMap.KeyToIndex(keyOfVisual);
                        //cba
                        if (kc != '\0')
                        {
                            adjOne[ic, ia] += 1;
                            Console.Write("1:" + kc.ToString()+ka.ToString()+" ");
                        }

                        if (kb != '\0')
                        {
                            adjZero[ib, ia] += 1;
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

                if (_symbolMap.SignsVisual.Contains(cr))
                {
                    key = _symbolMap.VisualToKey(cr);
                    kc = kb;
                    kb = ka;
                    ka = key;

                    ic = ib;
                    ib = ia;
                    ia = _symbolMap.KeyToIndex(key);
                    
                    AddCountData(_symbolMap.KeyToIndex(key));
                    //Fir: a = F
                    //b=F
                    //a=i
                    //c=F, b=i, a=r
                    if (kc != '\0')
                    {
                        adjOne[ic, ia] += 1;
                        Console.Write("1:" + kc.ToString()+ka.ToString()+" ");
                    }

                    if (kb != '\0')
                    {
                        adjZero[ib, ia] += 1;
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