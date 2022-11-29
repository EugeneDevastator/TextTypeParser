public interface IDataContainer
{
    float GetAdjMetric(char a, char b);
    void Fill(int[] keyCounts, int[,] adjZeroDir, int[,] adjOneDir);
    void SetSymbols(string symbols);
    void SaveToFolder(string folder);
    void LoadFromFolder(string folder);
}