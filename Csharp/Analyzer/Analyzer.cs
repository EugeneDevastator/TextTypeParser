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
    private CharData.CharData _charData;

    public Analyzer()
    {
        adjacency = np.load(Path.Combine(Constants.rootPath, Constants.AdjDatafile + ".npy"));
        adjacencyAny = np.load(Path.Combine(Constants.rootPath, Constants.BiAdjDatafile + ".npy"));
        counts = np.load(Path.Combine(Constants.rootPath, Constants.CountsDatafile + ".npy"));
        for (int i = 0; i < Constants.typable.Length; i++)
        {
            typIndices[Constants.typable[i]] = (byte)i;
        }
        _charData = new CharData.CharData();
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
        int xrange = 1;

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
                    //k!=0 will take horizonatals out of calculations.
                    if (IsValidChar(x + i, y + k) && k != 0)
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
            "*****_*****",
        };

        string[] prio = new string[3]
        {
            //  "34552025543",
            //  "00003_30000",
            //  "54331_13345",
            "27883_38872",
            "99995_59999",
            "24580_08542",
        };
//there is one more problem that we will fill one half first, due to increased entropy after first key is placed.

        string unuse = " ";
        foreach (var c in unuse)
        {
            unparsedYet = unparsedYet.Replace(c.ToString(), String.Empty);
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
                    unparsedYet = unparsedYet.Replace(c.ToString(), String.Empty);
                }
                else
                    lCounts[i, k] = 0;
            }
        }

        // new algo:
        // 1. pick most used key
        // 1.2 get key locations for most prioritized batch;
        // 2. find location with least interactions for new key inside the batch.
        List<(int x, int y)> GetCandidatePositions()
        {
            var maxBatch = 0;
            foreach (var coord in chars.Coords())
            {
                if (chars[coord.x, coord.y] == NONE)
                    if (priority[coord.x, coord.y] is var p && p > maxBatch)
                        maxBatch = p;
            }

            List<(int x, int y)> batchCoords = new List<(int x, int y)>();
            foreach (var coord in chars.Coords())
            {
                if (chars[coord.x, coord.y] == NONE)
                    if (priority[coord.x, coord.y] is var p && p == maxBatch)
                        batchCoords.Add(coord);
            }

            return batchCoords;
        }

        void GenerateLayoutByLetterAndBatches()
        {
            while (!String.IsNullOrEmpty(unparsedYet))
            {
                var maxCount = 0;
                char nextC = '\0';
                foreach (var c in unparsedYet.ToCharArray())
                {
                    if ((int)counts[typIndices[c]] is var cnt && cnt > maxCount)
                    {
                        maxCount = cnt;
                        nextC = c;
                    }
                }

                var batch = GetCandidatePositions();
                var bestcoord = batch[0];
                float maxRate = 0;
                foreach (var coord in batch)
                {
                    var matchStatic = GetNearChars(coord.x, coord.y);
                    //return count as well.
                    var (selfchar, weight) = LessAdjacentForAllWMetric(matchStatic, nextC.ToString());
                    if (weight > maxRate)
                    {
                        maxRate = weight;
                        bestcoord = coord;
                    }
                }

                chars[bestcoord.x, bestcoord.y] = nextC;
                lCounts[bestcoord.x, bestcoord.y] = counts[typIndices[nextC]];
                unparsedYet = unparsedYet.Replace(nextC.ToString(), String.Empty);
            }
        }

        void GenerateLayoutByEmptyWeightFirst()
        {
            while (!String.IsNullOrEmpty(unparsedYet))
            {
                int nextx = -1;
                int nexty = -1;
                int maxVal = 0;
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < 11; i++)
                    {
                        if (chars[i, k] != NONE)
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
                var (mostFit, _) = LessAdjacentForAllWMetric(matchStatic, unparsedYet);
                unparsedYet = unparsedYet.Replace(mostFit.ToString(), String.Empty);
                chars[nextx, nexty] = mostFit;
                lCounts[nextx, nexty] = counts[typIndices[mostFit]];
            }
        }

        void PrintForTable()
        {
            for (int k = 0; k < 3; k++)
            {
                string line = "";
                for (int i = 0; i < 11; i++)
                {
                    line += _charData.NameOf(chars[i, k]) + " ";
                }

                Console.WriteLine(line);
            }
        }

        void PrintRaw()
        {
            for (int k = 0; k < 3; k++)
            {
                string line = "";
                for (int i = 0; i < 11; i++)
                {
                    line += chars[i, k];
                }

                Console.WriteLine(line);
            }
        }
        
        

        //GenerateLayoutByEmptyWeightFirst();
        GenerateLayoutByLetterAndBatches();
        PrintRaw();
        Console.WriteLine();
        PrintForTable();

        Console.WriteLine("leftover:" + unparsedYet);
    }


    //bigger the number = lower the adjacency rating over count.
    public (char, float) LessAdjacentForAllWMetric(string setStatic, string setLook)
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
            sumCounts[i] = (cnt * cnt) / sumCounts[i];
        }

        var sortedSumIds = np.asarray(sumCounts).argsort<float>();
        var sortedChars = sortedSumIds.ToArray<int>().Select(i => lookChars[i]).ToArray();
        var sortedSums = np.asarray(sumCounts)[sortedSumIds];
        Console.WriteLine("adjacency sums to: " + setStatic);
        Console.WriteLine(sortedSums.ToString());
        Console.WriteLine(sortedChars);
        Console.WriteLine("last sum:" + sortedSums.GetSingle(-1));
        return (sortedChars[^1], sortedSums.GetSingle(-1)); //getting max;
    }

    public (char, float) LessAdjacentForAll(string setStatic, string setLook)
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
        return (sortedChars[0], sortedSums.GetSingle(0));
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