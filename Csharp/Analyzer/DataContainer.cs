using System.IO;
using CharData;
using MainApp;
using NumSharp;

public class DataContainer
{
    public NDArray adjacencyZero;
    public NDArray adjacencyOne;
    public NDArray counts;

    public NDArray adjacencyOneAny;
    public NDArray adjacencyZeroAny;
    
    public NDArray adjacencyMetric;
    public string _keys;

    public DataContainer()
    {
        adjacencyZero = np.load(Path.Combine(Constants.rootPath, Constants.AdjZeroDatafile));
        adjacencyOne = np.load(Path.Combine(Constants.rootPath, Constants.AdjOneDatafile));
        counts = np.load(Path.Combine(Constants.rootPath, Constants.CountsDatafile));
        _keys = File.ReadAllText(Path.Combine(Constants.rootPath, Constants.KeySetData));

        adjacencyZeroAny = adjacencyZero.copy();
        adjacencyOneAny = adjacencyOne.copy();
        adjacencyMetric = adjacencyOne.copy();
        
        //generate melt adjacencies;
        for (int k = 0; k < adjacencyOne.shape[1]; k++)
        {
            for (int i = k+1; i < adjacencyOne.shape[0]; i++)
            {
                adjacencyOneAny[i, k] += adjacencyOneAny[k, i];
                adjacencyOneAny[k, i] = adjacencyOneAny[i, k];
                
                adjacencyZeroAny[i, k] += adjacencyZeroAny[k, i];
                adjacencyZeroAny[k, i] = adjacencyZeroAny[i, k];
            }
        }
        
        GenerateMetricAdj();
    }

    private void GenerateMetricAdj()
    {
        for (int k = 0; k < adjacencyOne.shape[1]; k++)
        {
            for (int i = k + 1; i < adjacencyOne.shape[0]; i++)
            {
                adjacencyMetric[i, k] = adjacencyZeroAny[i, k] + adjacencyOneAny[i, k]*0;
            }
        }
    }
}

public class Charred2DArray<T> where T : unmanaged
{
    private NDArray _data;
    private char[] _chars;
    private KeyData _keyData;

    public Charred2DArray(NDArray data, KeyData keyData)
    {
        _keyData = keyData;
        _data = data;
    }

    public T this[char c1, char c2] => _data.GetValue<T>(new []{_keyData.IdxOf(c1), _keyData.IdxOf(c2)});

}