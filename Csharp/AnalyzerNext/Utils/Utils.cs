namespace AnalyzerNext;

public static class Utils
{
    public static string Expel(string source, params string[] exiled)
    {
        var exile = string.Join("", exiled);
        return new string(source.Where(k => !exile.Contains(k)).ToArray());
    }

    public static string Expel(string source, char[] exiled)
    {
        var exile = string.Join("", exiled);
        return new string(source.Where(k => !exile.Contains(k)).ToArray());
    }
    
    public static string FillToCount(string source, int count)
    {
        var missing = count - source.Length;
        if (missing < 1)
            return source;
        
        source += new string(LayoutConfig.SKIP, missing);
        return source;
    }

    public static void Copy2D(char[,] source, char[,] dst)
    {
        for (int i = 0; i < source.GetLength(0); i++)
        {
            for (int k = 0; k < source.GetLength(1); k++)
            {
                dst[i, k] = source[i, k];
            }
        }
    }

    public static (string head, string tail) Decap(string src, int headCount)
    {
        return (src[..headCount], src[headCount..]);
    }
}