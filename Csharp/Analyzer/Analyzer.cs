using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using CharData;
using MainApp;
using NumSharp;

public class Analyzer
{
    private IReadOnlyDictionary<char, byte> typIndices => _keyData.TypIndices; //= new Dictionary<char, byte>();
    private KeyData _keyData;
    private NDArray adjacencyMetric => _data.adjacencyMetric;
    private NDArray counts => _data.counts;

    private DataContainer _data;

    public Analyzer()
    {
        _data = new DataContainer();
        _keyData = new KeyData(_data._keys);

        //  for (int i = 0; i < _keyData.typable.Length; i++)
        //  {
        //      typIndices[_keyData.typable[i]] = (byte)i;
        //  }
    }

    public void GenerateLayout()
    {
        string unparsedYet = _keyData.typable;

        string unuse = " \\-90/[]=";
        foreach (var c in unuse)
        {
            unparsedYet = unparsedYet.Replace(c.ToString(), String.Empty);
        }

        const char SKIP = '_';
        const char NONE = '*';

        string[] baseline = new string[]
        {
            "_***p*_*lu**_",
            "*arst*_*neio*",
            "_**c**_*****_",
        };

        string[] priorityTemplate = new string[]
        {
            //"07883_38870",
            //"99995_59999",
            //"24581_18542",
            
            //std template:
            //"07883_38870",
            //"99995_59999",
            //"24581_18542",
            "_17886_68871_",
            "199997_799991",
            "_23881_18832_",
            
        };


        var w = baseline[0].Length;
        var h = baseline.Length;

        char[,] chars = new char[w, h];
        byte[,] priority = new byte[w, h];
        int[,] lCounts = new int[w, h];

        // Initial Fill, want arrays in x,y indexing.
        for (int k = 0; k < h; k++)
        {
            for (int i = 0; i < w; i++)
            {
                var c = baseline[k][i];
                chars[i, k] = c;

                if (priorityTemplate[k][i] != SKIP)
                    priority[i, k] = Convert.ToByte(priorityTemplate[k][i].ToString()); //wtf char no longer converts?

                if (c != SKIP && c != NONE)
                {
                    lCounts[i, k] = counts[typIndices[c]];
                    unparsedYet = unparsedYet.Replace(c.ToString(), String.Empty);
                }
                else
                    lCounts[i, k] = 0;
            }
        }

        //scan ranges for next cell detection and nearby detection
        int yrange = 1;
        int xrange = 1;

        bool IsValid(int x, int y) => x >= 0 && x < w && y >= 0 && y < h && chars[x, y] != SKIP;

        bool IsValidChar(int x, int y) =>
            x >= 0 && x < w && y >= 0 && y < h && chars[x, y] != SKIP && chars[x, y] != NONE;
        
        bool IsValidEmpty(int x, int y) =>
            x >= 0 && x < w && y >= 0 && y < h && chars[x, y] != SKIP && chars[x, y] == NONE;

        int CellWeight(int x, int y)
        {
            int weight = 0;
            for (int i = -xrange; i <= xrange; i++)
            {
                for (int k = -yrange; k <= yrange; k++)
                {
                    if (IsValid(x + i, y + k))
                    {
                        weight += lCounts[x + i, y + k];
                    }
                }
            }

            weight += priority[x, y] * 100000000;
            return weight;
        }


        (int x, int y) center = (x: 2, y: 2);
        string[] fitMeterTemplateRC = new string[]
        {
            "*LLL*",
            "MMLMM",
            "MM*MM",
            "MMLMM",
            "*LLL*",
        };

        const char IGNOR = '*';
        const char MAXIMIZE = 'M';
        const char MINIMIZE = 'L';

        //bigger score = more acceptable key is.
        float MeterAdjacencyFitScore(char candidate, char neighbor, char mode)
        {
            switch (mode)
            {
                case IGNOR:
                    return 0;
                case MAXIMIZE:
                    return _data.adjacencyMetric[_keyData.IdxOf(candidate),_keyData.IdxOf(neighbor)];
                case MINIMIZE:
                    var res = -1*_data.adjacencyMetric.GetSingle(new[]{_keyData.IdxOf(candidate),_keyData.IdxOf(neighbor)});
                    return res;
                default:
                    return 0;
            }
        }

        float GetAdjacencyScore(int x, int y, char candidate)
        {
            float score = 0;
            for (int i = -xrange; i <= xrange; i++)
            {
                for (int k = -yrange; k <= yrange; k++)
                {
                    if (IsValidChar(x + i, y + k))
                    {
                        var mode = fitMeterTemplateRC[center.y + k][center.x + i]; //not converted in RC format.
                        score += MeterAdjacencyFitScore(candidate, chars[i + x, k + y], mode);
                    }
                }
            }
            return score;
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
                if (batch.Count < 1)
                    break;

                var bestcoord = batch[0];
                float maxRate = 0;
                foreach (var coord in batch)
                {
                    //previous matcher
                    // var matchStatic = GetNearChars(coord.x, coord.y);
                    // var (selfchar, weight) = LessAdjacentForAllWMetric(matchStatic, nextC.ToString());
                    
                    var weight = GetAdjacencyScore(coord.x, coord.y, nextC);
                    
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
                for (int k = 0; k < h; k++)
                {
                    for (int i = 0; i < w; i++)
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
            for (int k = 0; k < h; k++)
            {
                string line = "";
                for (int i = 0; i < w; i++)
                {
                    line += _keyData.NameOf(chars[i, k]) + " ";
                }

                Console.WriteLine(line);
            }
        }

        void PrintRaw()
        {
            for (int k = 0; k < h; k++)
            {
                string line = "";
                for (int i = 0; i < w; i++)
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

        Console.WriteLine("leftover: " + unparsedYet);
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
                sumCounts[i] += adjacencyMetric[staticCharIds[k], lookCharIds[i]];
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
                sumCounts[i] += adjacencyMetric[staticCharIds[k], lookCharIds[i]];
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
            var row = adjacencyMetric[cIdx];
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