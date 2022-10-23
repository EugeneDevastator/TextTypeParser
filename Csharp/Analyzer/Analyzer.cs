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
        // vi:
       // Console.WriteLine(_data.adjacencyZero.GetSingle(new[]{_keyData.IdxOf('i'),_keyData.IdxOf('v')}));
       // return;
        
        string unparsedYet = _keyData.typable;

        string unuse = " \\-90[]=/";
        foreach (var c in unuse)
        {
            unparsedYet = unparsedYet.Replace(c.ToString(), String.Empty);
        }

        const char SKIP = '_';
        const char NONE = '*';

        string[] baseline = new string[]
        {
            //"_*hc*w_*lu**_",
            //"*arstf_*neio*",
            //"_***p*_*****_",
            "_*****_*****_",
            "_*rst*_*nei*_",
            "*a****_****o*",
        };


        string[] priorityTemplate = new string[]
        {
            //"107886_688701",
            //"1AABA7_7ABAA1",
            //"_23881_18832_",
            
            "018882",
            "08ABA8",
            "2A8888",
        };
        for (var i = 0; i < priorityTemplate.Length; i++)
        {
            priorityTemplate[i] = (priorityTemplate[i] + "_" + new String(priorityTemplate[i].Reverse().ToArray())).ToString();
        }
        
        string priokeys = "0123456789ABCDEF";
        Dictionary<char, byte> prioForKey = new Dictionary<char, byte>();
        priokeys.ToCharArray().Select((c, i) =>
        {
            prioForKey.Add(c, (byte)i);
            return 0;
        }).ToArray();
        
        string[] fitMeterTemplateMain = new string[]
        {
            "*LLL*",
            "*MLM*",
            "*M*M*",
            "*MLM*",
            "*LLL*",
        };
        
        string[] fitMeterTemplateLow = new string[]
        {
            "LLLLL",
            "**L**",
            "*M*M*",
            "**L**",
            "LLLLL",
        };
        string[] fitMeterTemplateUp = new string[]
        {
            "**L**",
            "**L**",
            "*****",
            "**L**",
            "**L**",
        };
        string[] fitMeterTemplatepinky = new string[]
        {
            "**L**",
            "*LLL*",
            "*L*L*",
            "*LLL*",
            "**L**",
        };

        string[] locationNames = new string[]
        {
            //"107886_688701",
            //"1AABA7_7ABAA1",
            //"_23881_18832_",
            
            "ppuuup",
            "pummmp",
            "pmuuup",
        };
        for (var i = 0; i < locationNames.Length; i++)
            locationNames[i] = (locationNames[i] + "_" + new String(locationNames[i].Reverse().ToArray())).ToString();
        
        Dictionary<char, string[]> patternAtLocation = new();
        patternAtLocation.Add('p',fitMeterTemplatepinky);
        patternAtLocation.Add('m',fitMeterTemplateMain);
        patternAtLocation.Add('u',fitMeterTemplateUp);
        patternAtLocation.Add('l',fitMeterTemplateLow);

        
        
        int yrange = (fitMeterTemplateMain.Length-1)/2;
        int xrange = (fitMeterTemplateMain[0].Length-1)/2;
        (int x, int y) center = (x: xrange, y: yrange);
        

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
                    priority[i, k] = prioForKey[priorityTemplate[k][i]];//Convert.ToHexString(priorityTemplate[k][i].ToString()); //wtf char no longer converts?

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

        const char IGNOR = '*';
        const char MAXIMIZE = 'M';
        const char MINIMIZE = 'L';
        const char MaxBefore = 'B';
        const char MinBefore = 'b';
        const char MaxAfter = 'A';
        const char MinAfter = 'a';

        //bigger score = more acceptable key is.
        float MeterAdjacencyFitScore(char candidate, char neighbor, char mode)
        {
            switch (mode)
            {
                //Djacency (after,before)
                
                case IGNOR:
                    return 0;
                case MAXIMIZE:
                    return 1*_data.adjacencyMetric.GetSingle(_keyData.IdxOf(candidate),_keyData.IdxOf(neighbor));
                case MINIMIZE:
                    return -1*_data.adjacencyMetric.GetSingle(_keyData.IdxOf(candidate),_keyData.IdxOf(neighbor));
                
                case MaxAfter: // marked after candidate
                    return 1*_data.adjacencyZero.GetSingle(_keyData.IdxOf(neighbor),_keyData.IdxOf(candidate));
                case MinAfter: 
                    return -1*_data.adjacencyZero.GetSingle(_keyData.IdxOf(neighbor),_keyData.IdxOf(candidate));
                
                case MaxBefore: // marked before candidate
                    return 1*_data.adjacencyZero.GetSingle(_keyData.IdxOf(candidate),_keyData.IdxOf(neighbor));
                case MinBefore: 
                    return -1*_data.adjacencyZero.GetSingle(_keyData.IdxOf(candidate),_keyData.IdxOf(neighbor));


                
                default:
                    return 0;
            }
        }

        ///
        /// returns normalized wieght: sum/count.
        float GetPlacementScoreForKey(int x, int y, char candidate)
        {
            float score = 0;
            float cellcount = 0;
            StringBuilder neighbors = new StringBuilder();
            var locationName = locationNames[y][x];
            var pattern = patternAtLocation[locationName];
            
            for (int i = -xrange; i <= xrange; i++)
            {
                for (int k = -yrange; k <= yrange; k++)
                {
                    if (IsValidChar(x + i, y + k))
                    {
                        var mode = pattern[center.y + k][center.x + i]; //not converted in RC format.
                        var neigh = chars[i + x, k + y];
                        if (mode != NONE)
                        {
                            score += MeterAdjacencyFitScore(candidate, neigh, mode);
                            neighbors.Append(neigh).Append(":" + mode).Append(" ");
                            cellcount++;
                        }
                    }
                }
            }

            Console.Write(candidate.ToString() + score / cellcount+" "+ neighbors.ToString() +"|");
            return score/cellcount;
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
                //GetBatch;
                var batch = GetCandidatePositions();
                if (batch.Count < 1)
                    break;

                //get batch symbols
                var maxCount = 0;
                var arr = unparsedYet.ToCharArray().OrderByDescending(c => counts.GetInt64(typIndices[c])).ToArray();
                var batchChars = arr[..batch.Count];

                var bestcoord = batch[0];
                var bestChar = batchChars[0];
                float maxRate = int.MinValue;
                
                foreach (var bc in batchChars)
                {
                    foreach (var coord in batch)
                    {
                        //previous matcher
                        // var matchStatic = GetNearChars(coord.x, coord.y);
                        // var (selfchar, weight) = LessAdjacentForAllWMetric(matchStatic, nextC.ToString());

                        var weight = GetPlacementScoreForKey(coord.x, coord.y, bc);

                        if (weight > maxRate)
                        {
                            maxRate = weight;
                            bestcoord = coord;
                            bestChar = bc;
                        }
                    }
                }

                chars[bestcoord.x, bestcoord.y] = bestChar;
                lCounts[bestcoord.x, bestcoord.y] = counts[typIndices[bestChar]];
                unparsedYet = unparsedYet.Replace(bestChar.ToString(), String.Empty);
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