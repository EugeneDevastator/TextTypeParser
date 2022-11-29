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
    private string _keySymbols;
    private CSVExporter _exporter;
    private int _keyCount;
    private long totalPresses;
    private float[,] _adjMetric;

    public PocoDatacontainer()
    {
        _exporter = new CSVExporter();
    }

    public void SetSymbols(string symbols)
    {
        _keySymbols = symbols;
    }

  
    public void Fill(int[] keyCounts, int[,] adjZeroDir, int[,] adjOneDir)
    {
        _keyCounts = keyCounts;
        _adjZeroDir = adjZeroDir;
        _adjOneDir = adjOneDir;
    }

    public void SaveToFolder(string folder)
    {
        _exporter.WriteVector(Path.Combine(folder,CountsDatafile), _keyCounts);
        _exporter.WriteData(Path.Combine(folder,AdjZeroDatafile), _adjZeroDir);
        _exporter.WriteData(Path.Combine(folder,AdjOneDatafile), _adjOneDir);
        File.WriteAllText(Path.Combine(folder,KeySetData),_keySymbols);
    }
    
    public float GetAdjMetric(char a, char b)
    {
        throw new NotImplementedException();
    }

    public void LoadFromFolder(string folder)
    {
        _keyCounts = _exporter.ReadVector<int>(Path.Combine(folder,CountsDatafile));
        _adjZeroDir = _exporter.ReadData<int>(Path.Combine(folder,AdjZeroDatafile));
        _adjOneDir = _exporter.ReadData<int>(Path.Combine(folder,AdjOneDatafile));
        _keySymbols = File.ReadAllText(Path.Combine(folder,KeySetData));
        _keyCount = _keyCounts.Length;
        GenerateSecondOrderData();
    }

    private void GenerateSecondOrderData()
    {
        totalPresses = _keyCounts.Sum();
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
                
                _adjMetric[i, k] = _adjOneAny[i, k] / (float)totalPresses;
            }
        }
    }
    
    private void GenerateAdjAny()
    {
        
    }
}