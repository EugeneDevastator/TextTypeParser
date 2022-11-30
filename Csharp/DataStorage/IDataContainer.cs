public interface IDataContainer
{
    float GetAdjMetric(char a, char b);
    void SetKeys(string keys);
    void SaveToFolder(string folder);
    void LoadFromFolder(string folder);
    string Keys { get; }
    int[] KeyCounts { get; }
    void Fill(int[] keyCounts, int[,] adjZeroDir, int[,] adjOneDir, string keys);
}