public class SymbolMap
{
    public Dictionary<char, byte> _visualToUniqueIndex = new Dictionary<char, byte>();
    private Dictionary<char, char> _visualToKey = new Dictionary<char, char>();
    private string _keysOfVisuals;
    
    public string DistinctLowerKeys { get; private set; }
    public string AllVisualSymbols { get; private set; }
    public string AllProjectedLowerKeys => _keysOfVisuals;

    public IReadOnlyDictionary<char, byte> KeyIndices => _keyIndices;
    private Dictionary<char, byte> _keyIndices = new Dictionary<char, byte>();
    
    private Dictionary<char, string> visualNames = new Dictionary<char, string>()
    {
        { ' ', "spc" },
        { '.', "dot" },
        { ',', "coma" },
        { ';', "semi" },
        { '\'', "quot" },
        { '\\', "slsh" },
        { '-', "minus" },
        { '[', "lbr" },
        { ']', "rbr" },
        { '*', "str" },
        { '=', "eq" },
    };
    
    public string[] _keyNames;
    
    /// <summary>
    /// symbols that end words, cannot be typed right now
    /// </summary>
    public string WordSeparators = " ;.,[]{}()-+=/*\"?<>`";

    public SymbolMap(IEnumerable<Language> languages)
    {
        AllVisualSymbols = "";
        _keysOfVisuals = "";
        foreach (var l in languages)
        {
            AllVisualSymbols += l.Visuals;
            _keysOfVisuals += l.Keys;
        }
        
        DistinctLowerKeys = new string(_keysOfVisuals.Distinct().ToArray());
        _keyNames = new string[DistinctLowerKeys.Length];
        for (int i = 0; i < DistinctLowerKeys.Length; i++)
        {
            _keyNames[i] = visualNames.ContainsKey(DistinctLowerKeys[i]) ? visualNames[DistinctLowerKeys[i]] : DistinctLowerKeys[i].ToString();
            _keyIndices[DistinctLowerKeys[i]] = (byte)i;
        }
        
        for (var i = 0; i < AllVisualSymbols.Length; i++)
        {
            _visualToKey.Add(AllVisualSymbols[i], _keysOfVisuals[i]);
            _visualToUniqueIndex.Add(AllVisualSymbols[i],_keyIndices[_keysOfVisuals[i]]);
        }
    }

    public char VisualToKey(char v) => _visualToKey[v];
    public byte KeyToIndex(char k) => _keyIndices[k];
    public byte VisToIndex(char v) => _visualToUniqueIndex[v] ;//GetKeyIndex(VisualToKey(v));
    public string NameOfKey(char k) => HasKey(k) ? _keyNames[KeyToIndex(k)] : k.ToString();
    public string NameOfKeyIndex(byte idx) => _keyNames[idx];
    public bool HasVisual(char v) => _visualToKey.ContainsKey(v);
    public bool HasKey(char k) => _keyIndices.ContainsKey(k);
}