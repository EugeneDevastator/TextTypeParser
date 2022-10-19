using System.IO;
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
                adjacencyMetric[i, k] = adjacencyZeroAny[i, k] + adjacencyOneAny[i, k];
            }
        }
    }
}