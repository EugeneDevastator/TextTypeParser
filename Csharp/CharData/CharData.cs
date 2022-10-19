namespace CharData;

public class CharData
{
    public const string visual = "abcdefghijklmnopqrstuvwxyz .,;"; //parser related
    public const string visualToTypable = ""; // parser
    
    public const string typable = "abcdefghijklmnopqrstuvwxyz .,;"; //stored data.
    public string[] typableNames;
    
    private Dictionary<char, byte> typIndices = new Dictionary<char, byte>();
    
    public CharData()
    {
        typableNames = new string[typable.Length];

        for (int i = 0; i < typable.Length; i++)
        {
            typableNames[i] = typable[i].ToString();
            typIndices[typable[i]] = (byte)i;
        }
        typableNames[^1] = "semi";
        typableNames[^2] = "coma";
        typableNames[^3] = "dot";
        typableNames[^4] = "spc";
    }

    public string NameOf(char c) => typIndices.ContainsKey(c) ? typableNames[typIndices[c]] : c.ToString();
    public int IdxOf(char c) => typIndices[c];
    public int CharAt(int id) => typable[id];
}

public class CharIndexedData<T>
{
    private readonly Array _data;
    private string _orderedChars;

    public CharIndexedData(Array data,  string orderedChars)
    {
        _data = data;
        _orderedChars = orderedChars;
    }
}
