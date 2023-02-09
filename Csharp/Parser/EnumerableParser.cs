using System.Text;
using MoreLinq;

public class EnumerableParser
{
    private int[,] adjZero;
    private int[,] adjOne;
    private int[] keyCounts;

    private string keys;
    private SymbolMap _symbols;
    private WordSplitter _worder;
    private IDataContainer _data;
    private ParseParams _parseParams;
    
    public EnumerableParser(IDataContainer data, ParseParams parseParams)
    {
        _parseParams = parseParams;
        _data = data;
        _symbols = new SymbolMap(parseParams.Languages);
        keys = _symbols.DistinctLowerKeys;
        Console.WriteLine(_symbols.DistinctLowerKeys.ToString());

        keyCounts = new int[keys.Length];
        adjZero = new int[keys.Length, keys.Length];
        adjOne = new int[keys.Length, keys.Length];

        for (int i = 0; i < keys.Length; i++)
        {
            keyCounts[i] = 0;
        }
    }
    
    public void Parse()
    {
        DirectoryInfo d = new DirectoryInfo(_parseParams.ParsingPath); //Assuming Test is your Folder
        FileInfo[] Files = d.GetFiles("*"); //Getting Text files
        var text = GetAllData(Files);

        //debug
        GetWordsInText(text, _symbols.AllVisualSymbols, _symbols.AllVisualSymbols)
            .Take(20)
            .ForEach(w=>Console.WriteLine(w));
        
        var words = GetWordsInText(text, _symbols.AllVisualSymbols, _symbols.AllVisualSymbols).AsParallel();

        if (_parseParams.Flags.HasFlag(WorderFlags.SimpleText))
            words.ForAll(w=> GetAdjacencies(w).ForEach(WriteAdjacency));

        if (_parseParams.Flags.HasFlag(WorderFlags.IntelliSense))
            words.ForAll(w =>
                GetIntellisenseWord(w, _symbols.AllProjectedLowerKeys.ToUpper().Distinct())
                    .Apply(GetAdjacencies)
                    .ForEach(WriteAdjacency));
                
        if (_parseParams.Flags.HasFlag(WorderFlags.First4Letters))
            words.ForAll(w=> GetAdjacencies(GetNFirstOfAWord(w,4)).ForEach(WriteAdjacency));

        WriteDataFiles();
    }

    private void WriteDataFiles()
    {
        //TODO delegate to datasource
        _data.Fill(keyCounts,adjZero,adjOne, _symbols);
        _data.SaveToFolder(_parseParams.DataPath);
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
    
    public IEnumerable<string> GetWordsInText(IEnumerable<char> source, IEnumerable<char> lettersOfWord, IEnumerable<char> allValidSymbols)
    {
        StringBuilder word = new();
        foreach (var ch in source)
        {
            if (!lettersOfWord.Contains(ch))
            {
                if (word.Length > 0)
                    yield return word.ToString();
                word.Clear();
                
                if (allValidSymbols.Contains(ch))
                {
                    yield return ch.ToString();
                    continue;
                }
            }
            
            if (allValidSymbols.Contains(ch))
                word.Append(ch);
        }
    }

    public IEnumerable<string> GetPascalSubWords(IEnumerable<char> multiword, IEnumerable<char> upperLetters)
    {
        StringBuilder subword = new();
        foreach (var ch in multiword)
        {
            subword.Append(ch);
            if (upperLetters.Contains(ch))
            {
                yield return subword.ToString();
                subword.Clear();
            }
        }
    }
    
    public string GetIntellisenseWord(IEnumerable<char> multiword, IEnumerable<char> upperLetters)
    {
        StringBuilder subword = new();
        foreach (var ch in multiword)
        {

            if (upperLetters.Contains(ch))
            {
                subword.Append(ch);
            }
        }
        return subword.ToString();
    }

    public string GetNFirstOfAWord(IEnumerable<char> word, int n)
    {
        var len = Math.Min(word.Count(), n);
        return new string(word.Take(5).ToArray());
    }

    public IEnumerable<AdjInfo> GetAdjacencies(string word)
    {
        char a = '\0';
        char b = '\0';
        char c = '\0';
        for (int i = 0; i < word.Length; i++)
        {
            yield return new AdjInfo(word[i]);
            if (i > 0)
                yield return new AdjInfo(word[i-1], word[i],0);
            if (i > 1)
                yield return new AdjInfo(word[i-2], word[i],1);
        }
    }

    private void WriteAdjacency(AdjInfo pair)
    {
        switch (pair.spread)
        {
            case -1:
                AddCountData(pair.a);
                break;
            case 0:
                AddAdjacencyZero(pair.a,pair.b);
                break;
            case 1:
                AddAdjacencyOne(pair.a,pair.b);
                break;
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
    
    private void AddCountData(char c)
    {
        keyCounts[IndexOfVis(c)] += 1;
    }
    private int IndexOfVis(char v) => _symbols.VisToIndex(v);
}