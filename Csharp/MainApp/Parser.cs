using System.Text;
using NumSharp;

public class Parser
{
    private const string parsePath = "D:\\1\\all";
    private const string rootPath = "D:\\1\\";
    private const string adjDirectName = "dirAdjacency.csv";
    private const string sortedDirectAdjName = "dirAdjacencySort.csv";
    private const string sortedBiDirAdjName = "BidirAdjacencySort.csv";
    private const string adjRawName = "absAdjacency.csv";
    private const string countsName = "rawCounts.csv";

    private const string BiAdjDatafile = "biAdj.data";
    private const string AdjDatafile = "Adj.data";
    private const string CountsDatafile = "counts.data";

    private const string typable = "abcdefghijklmnopqrstuvwxyz .,;";

    private char[] typableChars;
    private long[] typableCounts;
    private string[] typableNames;

    private float[,] adjFloat;
    private float[,] adjFloatAny;
    
    private NDArray adjacency;
    private NDArray adjacencyAny;

    //private float[,] adjacency;
    //private Dictionary<char, int> typableCounts = new Dictionary<char, int>();
    private Dictionary<char, byte> typIndices = new Dictionary<char, byte>();

    public Parser()
    {
        adjacency = np.zeros((typable.Length, typable.Length), NPTypeCode.Float);
        adjacencyAny = np.zeros((typable.Length, typable.Length), NPTypeCode.Float);
        typableNames = new string[typable.Length];
        typableCounts = new long[typable.Length];
        typableChars = typable.ToCharArray();
        adjFloat = new float[typable.Length, typable.Length];
        adjFloatAny = new float[typable.Length, typable.Length];
        
        for (int i = 0; i < typable.Length; i++)
        {
            typableCounts[i] = 0;
            typableNames[i] = typable[i].ToString();
            typIndices[typable[i]] = (byte)i;
        }

        typableNames[^1] = "semi";
        typableNames[^2] = "coma";
        typableNames[^3] = "dot";
        typableNames[^4] = "spc";
    }

    public void Parse()
    {
        DirectoryInfo d = new DirectoryInfo(parsePath); //Assuming Test is your Folder
        FileInfo[] Files = d.GetFiles("*"); //Getting Text files

        ExtractDataToVars(Files);
        WriteDataFiles();
        WriteSortedAdjacency();
        WriteSortedAdjacencyAny();
        WriteAdjacency();
        WriteAdjacencyAny();
        WriteCounts();
    }

    private void WriteDataFiles()
    {
        for (int i = 0; i < typable.Length; i++)
        {
            for (int k = 0; k < typable.Length; k++)
            {
                adjacency[i, k] = adjFloat[i, k];
                adjacencyAny[i, k] = adjFloatAny[i, k];
            }
        }
        
        np.save(Path.Combine(rootPath, AdjDatafile), adjacency);
        np.save(Path.Combine(rootPath, BiAdjDatafile), adjacencyAny);
        np.save(Path.Combine(rootPath,CountsDatafile),np.asarray(typableCounts));
    }

    private void WriteAdjacency()
    {
        StringBuilder output = new StringBuilder();
        output.Append("DIR ADJ;");
        for (int k = 0; k < typable.Length; k++)
            output.Append(typableNames[k]).Append(";");
        output.Append("\n");

        for (int k = 0; k < typable.Length; k++)
        {
            output.Append(typableNames[k]).Append(";");
            for (var i = 0; i < typable.Length; i++)
            {
                output.Append(adjacency[i, k].ToString()).Append(";");
            }

            output.Append("\n");
        }

        File.WriteAllText(Path.Combine(rootPath, adjDirectName), output.ToString());
    }

    private void WriteAdjacencyAny()
    {
        StringBuilder output = new StringBuilder();
        output.Append("BIDIR ADJ;");
        for (int k = 0; k < typable.Length; k++)
            output.Append(typableNames[k]).Append(";");
        output.Append("\n");

        for (int k = 0; k < typable.Length; k++)
        {
            output.Append(typableNames[k]).Append(";");
            for (var i = 0; i < typable.Length; i++)
            {
                output.Append(adjacencyAny[i, k].ToString()).Append(";");
            }
            output.Append("\n");
        }

        File.WriteAllText(Path.Combine(rootPath, adjRawName), output.ToString());
    }
    
    private void WriteSortedAdjacency()
    {
        StringBuilder output = new StringBuilder();
        output.Append("SORT DIR ADJ;");

        for (int k = 0; k < typable.Length; k++)
        {
            var idx = adjacency[k].argsort<float>();
            var sVals = adjacency[k][idx];

            output.Append(typableNames[k]).Append(";");
            for (int i = 0; i < typable.Length; i++)
                output.Append(typableNames[idx[i]]).Append(";");
            output.Append('\n');

            output.Append(typableNames[k]).Append(";");
            for (int i = 0; i < typable.Length; i++)
                output.Append(sVals[i].ToString()).Append(";");
            output.Append('\n');
        }

        File.WriteAllText(Path.Combine(rootPath, sortedDirectAdjName), output.ToString());
    }

    private void WriteSortedAdjacencyAny()
    {
        StringBuilder output = new StringBuilder();
        output.Append("SORT bidir ADJ;");

        for (int k = 0; k < typable.Length; k++)
        {
            //sorted indices and vals
            var sIdx = adjacencyAny[k].argsort<float>();
            var sVals = adjacencyAny[k][sIdx];

            output.Append(typableNames[k]).Append(";");
            for (int i = 0; i < typable.Length; i++)
                output.Append(typableNames[sIdx[i]]).Append(";");
            output.Append('\n');

            output.Append(typableNames[k]).Append(";");
            for (int i = 0; i < typable.Length; i++)
                output.Append(sVals[i].ToString()).Append(";");
            output.Append('\n');
        }

        File.WriteAllText(Path.Combine(rootPath, sortedBiDirAdjName), output.ToString());
    }

    private void WriteCounts()
    {
        StringBuilder output = new StringBuilder();

        for (int k = 0; k < typable.Length; k++)
        {
            output.Append(typableNames[k]).Append(";").Append(typableCounts[k]).Append("\n");
        }

        File.WriteAllText(Path.Combine(rootPath, countsName), output.ToString());
    }

    private void ExtractDataToVars(FileInfo[] Files)
    {
        float total = Files.Length;
        int curr = 0;
        char c;
        StringBuilder allofthem = new StringBuilder();
        foreach (var f in Files)
        {
            if (curr % 20 == 0)
                Console.WriteLine("progress" + curr / total);

            curr++;
//            if (curr > 200)
//                break;
            
            var content = File.ReadAllText(f.FullName);
            allofthem.Append(content);
        }
        
//        foreach (var f in Files)
        {
            int pos = 0;
            char ka = '\0', kb = '\0', kc = '\0';
            byte ia = 0, ib = 0, ic = 0;
            // using (StreamReader sr = new StreamReader(f.FullName, Encoding.Default))
            // {
            //     while (sr.Peek() >= 0)
            //     {
            var content = allofthem.ToString();//File.ReadAllText(f.FullName);
            float totalSize = content.Length;
            foreach (var cr in content.ToCharArray())
            {
                pos++;
                if (pos % 400000 ==0)
                    Console.WriteLine(pos/totalSize);
                //not really interested in uppercasing.
                c = char.ToLower(cr);
                if (typableChars.Contains(c))
                {
                    kc = kb;
                    kb = ka;
                    ka = c;
                    
                    ic = ib;
                    ib = ia;
                    ia = typIndices[c];
                    
                    if (kc != '\0')
                    {
                        AddAdjacencyData(ia, ib, ic);
                        AddCountData(ic);
                    }
                }
                else
                {
                    kc = ka = kb = '\0';
                }
            }
        }
    }

    private void AddAdjacencyData(byte ia, byte ib, byte ic)
    {
        //so turns out numpy sucks at setting values..
        adjFloat[ia, ic] += 0.1f;
        adjFloat[ib, ic] += 1;

        adjFloatAny[ia, ic] += 0.1f;
        adjFloatAny[ib, ic] += 1;
        adjFloatAny[ic, ia] += 0.1f;
        adjFloatAny[ic, ib] += 1;
    }

    private void AddCountData(byte idx)
    {
        typableCounts[idx] += 1;
    }
}