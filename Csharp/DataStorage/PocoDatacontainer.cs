using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using MoreLinq;

public class PocoDatacontainer : IDataContainer
{
    public const string AdjOneDatafile = "AdjOne.csv";
    public const string AdjZeroDatafile = "AdjZero.csv";
    public const string CountsDatafile = "counts.csv";
    public const string KeySetData = "keyset.txt";
    public const string LanguageData = "language.json";
    
    private int[,] _adjZeroDir; // y follows x
    private int[,] _adjOneDir;
    private int[,] _adjZeroAny; // y near x
    private int[,] _adjOneAny;
    private int[] _keyCounts;
    private string _keys;
    private CSVExporter _exporter;
    private int _keyCount;
    private long totalPresses;
    private float[,] _adjMetric;
    private Dictionary<char, int> _keysToId=new();
    private string _lowerLetterLanguage;
    private SymbolMap _symbolMap;
    
    TextEncoderSettings encoderSettings = new TextEncoderSettings();

    public PocoDatacontainer()
    {
        _exporter = new CSVExporter();
        encoderSettings.AllowRange(UnicodeRanges.Cyrillic);
        encoderSettings.AllowRange(UnicodeRanges.BasicLatin);
    }

    public string Keys => _keys;
    public int[] KeyCounts => _keyCounts;
    public Dictionary<char,int> CountPerKey => _keyCounts
        .Select((k, id)=> (k,id))
        .ToDictionary(p=>_keys[p.id],p=> p.k);

    public string LowerLetterLanguage => _lowerLetterLanguage;

    public SymbolMap SymbolMap => _symbolMap;

    public void SetKeys(string keys)
    {
        _keys = keys;
    }

    public int GetKeyCount(char k) => _keyCounts[_keysToId[k]];
  
    public void Fill(int[] keyCounts, int[,] adjZeroDir, int[,] adjOneDir, SymbolMap symbolMap)
    {
        _symbolMap = symbolMap;
        _keyCounts = keyCounts;
        _adjZeroDir = adjZeroDir;
        _adjOneDir = adjOneDir;
    }

    public void SaveToFolder(string folder)
    {
        _exporter.WriteVector(Path.Combine(folder,CountsDatafile), KeyCounts);
        _exporter.WriteData(Path.Combine(folder,AdjZeroDatafile), _adjZeroDir);
        _exporter.WriteData(Path.Combine(folder,AdjOneDatafile), _adjOneDir);
        File.WriteAllText(Path.Combine(folder,KeySetData),_symbolMap.DistinctLowerKeys);

        var language = new Language() { Visuals = _symbolMap.AllVisualSymbols, Keys = _symbolMap.AllProjectedLowerKeys };
        var serialized = JsonSerializer.Serialize(language);
        language.Write(Path.Combine(folder,LanguageData));

        _keys = _symbolMap.DistinctLowerKeys;
        for (var i = 0; i < _symbolMap.DistinctLowerKeys.Length; i++)
        {
            _keysToId.Add(_keys[i],i);
        }
        
        var s = Keys.Select(k => (k, GetKeyCount(k))).ToArray();
        StringBuilder str = new StringBuilder();
        foreach (var entry in s)
        {
            str.Append(entry.k).Append(";").Append(entry.Item2).Append("\n");
        }
        File.WriteAllText(Path.Combine(folder,KeySetData+"_View"),str.ToString());
    }

    public float GetAdjMetric(char a, char b) => _adjMetric[_keysToId[a], _keysToId[b]];

    public void LoadFromFolder(string folder, float adjOneMul = 0f)
    {
        _keyCounts = _exporter.ReadVector<int>(Path.Combine(folder,CountsDatafile));
        _adjZeroDir = _exporter.ReadData<int>(Path.Combine(folder,AdjZeroDatafile));
        _adjOneDir = _exporter.ReadData<int>(Path.Combine(folder,AdjOneDatafile));
        _keys = File.ReadAllText(Path.Combine(folder,KeySetData));
        var dataLanguage = Language.ReadFromFile(Path.Combine(folder,LanguageData));
        _symbolMap = new SymbolMap(new [] {dataLanguage});
        
        _keyCount = _keyCounts.Length;
        
        GenerateSecondOrderData(adjOneMul);
        

    }

    private void GenerateSecondOrderData(float adjOneMul)
    {
        for (var i = 0; i < _keys.Length; i++)
        {
            _keysToId.Add(_keys[i],i);
        }
        
        totalPresses = KeyCounts.Sum();
        _adjOneAny = new int[_keyCount, _keyCount];
        _adjZeroAny = new int[_keyCount, _keyCount];
        _adjMetric = new float[_keyCount, _keyCount];

        for (int k = 0; k < _keyCount; k++)
        {
            for (int i = 0 ; i < _keyCount; i++)
            {
                if (i != k)
                {
                    _adjOneAny[i, k] = _adjOneDir[k, i] + _adjOneDir[i, k];
                    _adjZeroAny[i, k] = _adjZeroDir[k, i] + _adjZeroDir[i, k];
                }
                else
                {
                    _adjOneAny[i, k] = _adjOneDir[i, k];
                    _adjZeroAny[i, k] = _adjZeroDir[i, k];
                }

                _adjMetric[i, k] =
                    (_adjZeroAny[i, k] + adjOneMul * _adjOneAny[i, k]);// / totalPresses;
                    ; //;/ (float)totalPresses;
            }
        }
    }
}