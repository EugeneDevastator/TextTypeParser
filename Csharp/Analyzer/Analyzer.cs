using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using MainApp;
using NumSharp;

public class Analyzer
{
    private NDArray adjacency;
    private NDArray adjacencyAny;
    private NDArray counts;

    private Dictionary<char, byte> typIndices = new Dictionary<char, byte>();

    public Analyzer()
    {
        adjacency = np.load(Path.Combine(Constants.rootPath, Constants.AdjDatafile + ".npy"));
        adjacencyAny = np.load(Path.Combine(Constants.rootPath, Constants.BiAdjDatafile + ".npy"));
        counts = np.load(Path.Combine(Constants.rootPath, Constants.CountsDatafile + ".npy"));
        for (int i = 0; i < Constants.typable.Length; i++)
        {
            typIndices[Constants.typable[i]] = (byte)i;
        }
    }

    public void GenerateLayout()
    {
        const char SKIP = '_';
        const char NONE = '*';
        char[,] chars = new char[11, 3];
        byte[,] priority = new byte[11, 3];
        int[,] lCounts = new int[11, 3];
        
        //scan ranges for next cell detection and nearby detection
        int yrange = 2;
        int xrange = 2;
        
        bool IsValid(int x, int y) => x >= 0 && x < 11 && y >= 0 && y < 3 && chars[x, y] != SKIP;

        bool IsValidChar(int x, int y) =>
            x >= 0 && x < 11 && y >= 0 && y < 3 && chars[x, y] != SKIP && chars[x, y] != NONE;

        int CellWeight(int x, int y)
        {
            int w = 0;
            for (int i = -xrange; i <= xrange; i++)
            {
                for (int k = -yrange; k <= yrange; k++)
                {
                    if (IsValid(x + i, y + k))
                    {
                        w += lCounts[x + i, y + k];
                    }
                }
            }
            w += priority[x, y] * 100000000;
            return w;
        }

        string GetNearChars(int x, int y)
        {
            StringBuilder res = new StringBuilder();
            for (int i = -xrange; i <= xrange; i++)
            {
                for (int k = -yrange; k <= yrange; k++)
                {
                    if (IsValidChar(x + i, y + k))
                    {
                        res.Append(chars[x + i, y + k]);
                    }
                }
            }

            return res.ToString();
        }

        string unparsedYet = Constants.typable;

        string[] baseline = new string[3]
        {
            "*****_*****",
            "arst*_*neio",
            ",****_****.",
        };
        
        string[] prio = new string[3]
        {
            "34552025543",
            "00003_30000",
            "03331_13330",
        };

        
        string unuse = " ";
        foreach (var c in unuse)
        {
            unparsedYet=unparsedYet.Replace(c.ToString(),String.Empty);
        }

        // Initial Fill, dont want to index arrays in row-column format.
        for (int k = 0; k < 3; k++)
        {
            for (int i = 0; i < 11; i++)
            {
                var c = baseline[k][i];
                chars[i, k] = c;
                priority[i, k] = Convert.ToByte(prio[k][i]);
                if (c != SKIP && c != NONE)
                {
                    lCounts[i, k] = counts[typIndices[c]];
                    unparsedYet=unparsedYet.Replace(c.ToString(),String.Empty);
                }
                else
                    lCounts[i, k] = 0;
            }
        }

        while (!String.IsNullOrEmpty(unparsedYet))
        {

            int nextx = -1;
            int nexty = -1;
            int maxVal = 0;
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < 11; i++)
                {
                    if (chars[i, k] !=  NONE)
                        continue;
                    
                    var w = CellWeight(i, k);
                    if (w > maxVal)
                    {
                        maxVal = w;
                        nextx = i;
                        nexty = k;
                    }
                }
            }

            if (nextx < 0 || nexty < 0)
                break;
            
            var matchStatic = GetNearChars(nextx, nexty);
            var mostFit = LessAdjacentForAllWMetric(matchStatic, unparsedYet);
            unparsedYet= unparsedYet.Replace(mostFit.ToString(),String.Empty);
            chars[nextx, nexty] = mostFit;
            lCounts[nextx, nexty] = counts[typIndices[mostFit]];
        }

        for (int k = 0; k < 3; k++)
        {
            string line = "";
            for (int i = 0; i < 11; i++)
            {
                line += chars[i, k];
            }
            Console.WriteLine(line);
        }
        Console.WriteLine("leftover:"+unparsedYet);
    }


    public char LessAdjacentForAllWMetric(string setStatic, string setLook)
    {
        var staticChars = setStatic.ToCharArray();
        var lookChars = setLook.ToCharArray();
        var staticCharIds = setStatic.ToCharArray().Select(c => typIndices[c]).ToArray();
        var lookCharIds = setLook.ToCharArray().Select(c => typIndices[c]).ToArray();

        float[] sumCounts = new float[setLook.Length];

        for (int i = 0; i < setLook.Length; i++)
        {
            sumCounts[i] = 0;
            for (int k = 0; k < setStatic.Length; k++)
            {
                sumCounts[i] += adjacencyAny[staticCharIds[k], lookCharIds[i]];
            }

            var cnt = counts.GetInt64(lookCharIds[i]);
            sumCounts[i] = (cnt*cnt)/ sumCounts[i];
        }

        var sortedSumIds = np.asarray(sumCounts).argsort<float>();
        var sortedChars = sortedSumIds.ToArray<int>().Select(i => lookChars[i]).ToArray();
        var sortedSums = np.asarray(sumCounts)[sortedSumIds];
        Console.WriteLine("adjacency sums to: " + setStatic);
        Console.WriteLine(sortedSums.ToString());
        Console.WriteLine(sortedChars);
        return sortedChars[^1]; //getting max;
    }
    
    public char LessAdjacentForAll(string setStatic, string setLook)
    {
        var staticChars = setStatic.ToCharArray();
        var lookChars = setLook.ToCharArray();
        var staticCharIds = setStatic.ToCharArray().Select(c => typIndices[c]).ToArray();
        var lookCharIds = setLook.ToCharArray().Select(c => typIndices[c]).ToArray();

        float[] sumCounts = new float[setLook.Length];

        for (int i = 0; i < setLook.Length; i++)
        {
            sumCounts[i] = 0;
            for (int k = 0; k < setStatic.Length; k++)
            {
                sumCounts[i] += adjacencyAny[staticCharIds[k], lookCharIds[i]];
            }
        }

        var sortedSumIds = np.asarray(sumCounts).argsort<float>();
        var sortedChars = sortedSumIds.ToArray<int>().Select(i => lookChars[i]).ToArray();
        var sortedSums = np.asarray(sumCounts)[sortedSumIds];
        Console.WriteLine("adjacency sums to: " + setStatic);
        Console.WriteLine(sortedSums.ToString());
        Console.WriteLine(sortedChars);
        return sortedChars[0];
    }

    //find sorted char adjacencies to each one from static set.
    public void LessAdjacentForEach(string setStatic, string setLook)
    {
        var staticChars = setStatic.ToCharArray();
        var lookChars = setLook.ToCharArray();
        StringBuilder output = new StringBuilder();
        foreach (var cStat in staticChars)
        {
            var cIdx = typIndices[cStat];
            var lookIndices = lookChars.Select(c => typIndices[c]).ToArray();
            var row = adjacencyAny[cIdx];
            var lookedAdj = row[np.asarray(lookIndices)];
            var localIdxSort = lookedAdj.argsort<float>();

            var sortedchars = localIdxSort.ToArray<int>().Select(i => lookChars[i]).ToArray();
            Console.WriteLine(cStat + ": " + lookedAdj[localIdxSort].ToString());
            Console.WriteLine(sortedchars);


            /*char minchar = '\0';
            float minAdj = -1;
            foreach (var cLook in setLook.ToCharArray())
            {
                if (adjacencyAny[cStat][setIndices] < minAdj)
                {
                    minAdj = adjacencyAny[cStat, cLook];
                    minchar = cLook;
                }
            }
            output.Append(cStat).Append("\n").Append(" min =>" )*/
        }
    }
}