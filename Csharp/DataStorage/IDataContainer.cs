public interface IDataContainer
{
    float GetAdjMetric(char a, char b);
    void SetKeys(string keys);
    void SaveToFolder(string folder);
    void LoadFromFolder(string folder, float adjOneMul = 0f);
    string Keys { get; }
    int[] KeyCounts { get; }
    SymbolMap SymbolMap { get; }
    Dictionary<char, int> CountPerKey { get; }
    void Fill(int[] keyCounts, int[,] adjZeroDir, int[,] adjOneDir, string keys, string language);
    int GetKeyCount(char k);
}