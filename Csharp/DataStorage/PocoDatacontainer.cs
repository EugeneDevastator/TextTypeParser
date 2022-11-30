public class PocoDatacontainer : IDataContainer
{
    public const string AdjOneDatafile = "AdjOne.csv";
    public const string AdjZeroDatafile = "AdjZero.csv";
    public const string CountsDatafile = "counts.csv";
    public const string KeySetData = "keyset.txt";
    
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

    public PocoDatacontainer()
    {
        _exporter = new CSVExporter();
    }

    public string Keys => _keys;
    public int[] KeyCounts => _keyCounts;

    public void SetKeys(string keys)
    {
        _keys = keys;
    }

  
    public void Fill(int[] keyCounts, int[,] adjZeroDir, int[,] adjOneDir, string keys)
    {
        _keyCounts = keyCounts;
        _adjZeroDir = adjZeroDir;
        _adjOneDir = adjOneDir;
        _keys = keys;
    }

    public void SaveToFolder(string folder)
    {
        _exporter.WriteVector(Path.Combine(folder,CountsDatafile), KeyCounts);
        _exporter.WriteData(Path.Combine(folder,AdjZeroDatafile), _adjZeroDir);
        _exporter.WriteData(Path.Combine(folder,AdjOneDatafile), _adjOneDir);
        File.WriteAllText(Path.Combine(folder,KeySetData),Keys);
    }

    public float GetAdjMetric(char a, char b) => _adjMetric[_keysToId[a], _keysToId[b]];

    public void LoadFromFolder(string folder)
    {
        _keyCounts = _exporter.ReadVector<int>(Path.Combine(folder,CountsDatafile));
        _adjZeroDir = _exporter.ReadData<int>(Path.Combine(folder,AdjZeroDatafile));
        _adjOneDir = _exporter.ReadData<int>(Path.Combine(folder,AdjOneDatafile));
        _keys = File.ReadAllText(Path.Combine(folder,KeySetData));
        _keyCount = KeyCounts.Length;
        GenerateSecondOrderData();
    }

    private void GenerateSecondOrderData()
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

                _adjMetric[i, k] = _adjZeroAny[i, k] + _adjOneAny[i,k]; //;/ (float)totalPresses;
            }
        }
    }
}