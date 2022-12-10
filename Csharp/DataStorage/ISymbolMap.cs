public interface ISymbolMap
{
    IReadOnlyDictionary<char, byte> KeyIndices { get; }
    string KeyboardKeys { get; }
    string SignsVisual { get; }
    string LettersVisual { get; }
    char VisualToKey(char v);
    byte KeyToIndex(char k);
    byte VisToIndex(char v);
    string NameOfKey(char k);
    string NameOfKeyIndex(byte idx);
    bool HasVisual(char v);
    bool HasKey(char k);
}