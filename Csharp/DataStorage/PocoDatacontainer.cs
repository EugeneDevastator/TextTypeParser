public class PocoDatacontainer : IDataContainer
{
    public const string AdjOneDatafile = "AdjOne.csv";
    public const string AdjZeroDatafile = "AdjZero.csv";
    public const string CountsDatafile = "counts.csv";
    public const string KeySetData = "keyset.txt";
    
    private int[,] adjZeroDir; // y follows x
    private int[,] adjOneDir;
    
    private int[,] adjZeroAny; // y near x
    private int[,] adjOneAny;
    private int[] keyCounts;
    private string keySymbols;
    
    public float GetAdjMetric(char a, char b)
    {
        throw new NotImplementedException();
    }

    public void LoadFromFolder(string folder)
    {
        
    }

    private void GenerateAdjAny()
    {
        
    }
}