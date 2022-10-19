public class KeyConverter
{
    private string letters = "abcdefghijklmnopqrstuvwxyz";
    private string symbols = " 9(0):;" + '\"' + "'" + ",<.>/?-_=+[{]}" + '\\' + "|";
    private string symbolsKeys = " 9900;;" + "'',,..//--==[[]]" + '\\' + '\\';
        
    public string VisualSymbols;
    private string _visualToKeys;
    private Dictionary<char, int> _visualToIndex = new Dictionary<char, int>();
    public string UniqueKeys;

    public KeyConverter()
    {
        VisualSymbols = letters + letters.ToUpper() + symbols;
        _visualToKeys = letters + letters + symbolsKeys;
            
        for (var i = 0; i < VisualSymbols.Length; i++)
        {
            _visualToIndex.Add(VisualSymbols[i], i);
        }

        UniqueKeys = new string(_visualToKeys.Distinct().ToArray());
    }

    public char VisualToKey(char v) => _visualToKeys[_visualToIndex[v]];
}