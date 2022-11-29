/// <summary>
/// Letter - raw semantic for letters of alphabet
/// Symbol - special character (non letter)
/// Sign - letter or symbol or space etc.
/// Key - meaning keyboard key in its unshifted variation
/// Visual - sign that we see in text (shifted + unshifted)
/// </summary>

public class SymbolMap
{
    public Dictionary<char, byte> _visualToUniqueIndex = new Dictionary<char, byte>();
    private Dictionary<char, char> _visualToKey = new Dictionary<char, char>();
    private string _keyForSign;
    

    
    public string KeyboardKeys;
    public string SignsVisual;
    public string LettersVisual;
    
    public string LettersUpper;
    public string LettersLower = "abcdefghijklmnopqrstuvwxyz";
    private string _symbolsVisual =     " 9(0):;" + '\"' + "\'" + ",<.>/?-_=+[{]}" + '\\' + "|";
    private string _symbolsKeys = " 9900;;" + "\'" + "\'" + ",,..//--==[[]]" + '\\' + '\\'; //non-shifted keys.

    public IReadOnlyDictionary<char, byte> KeyIndices => _keyIndices;
    private Dictionary<char, byte> _keyIndices = new Dictionary<char, byte>();
    
    private Dictionary<char, string> charNames = new Dictionary<char, string>()
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
    public string WordSeparators = " ;.,[]{}()-+=/*\"?<>";

    public SymbolMap()
    {
        LettersUpper = LettersLower.ToUpper();
        LettersVisual = LettersLower + LettersUpper;
        SignsVisual = LettersLower + LettersLower.ToUpper() + _symbolsVisual;
        _keyForSign = LettersLower + LettersLower + _symbolsKeys;
        
        KeyboardKeys = new string(_keyForSign.Distinct().ToArray());
        _keyNames = new string[KeyboardKeys.Length];
        for (int i = 0; i < KeyboardKeys.Length; i++)
        {
            _keyNames[i] = charNames.ContainsKey(KeyboardKeys[i]) ? charNames[KeyboardKeys[i]] : KeyboardKeys[i].ToString();
            _keyIndices[KeyboardKeys[i]] = (byte)i;
        }
        
        for (var i = 0; i < SignsVisual.Length; i++)
        {
            _visualToKey.Add(SignsVisual[i], _keyForSign[i]);
            _visualToUniqueIndex.Add(SignsVisual[i],_keyIndices[_keyForSign[i]]);
        }
    }

    public char VisualToKey(char v) => _keyForSign[v];
    public byte KeyToIndex(char k) => _keyIndices[k];
    public byte VisToIndex(char v) => _visualToUniqueIndex[v] ;//GetKeyIndex(VisualToKey(v));
    public string NameOfKey(char k) => _keyNames[KeyToIndex(k)];
    public string NameOfKeyIndex(byte idx) => _keyNames[idx];
    public bool HasVisual(char v) => _visualToKey.ContainsKey(v);
}