using System.Text;
using AnalyzerNext;

namespace AnalyzerUtils;

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

    public static void Copy2D(ref char[,] source, ref char[,] dst)
    {
        for (int i = 0; i < source.GetLength(0); i++)
        {
            for (int k = 0; k < source.GetLength(1); k++)
            {
                dst[i, k] = source[i, k];
            }
        }
    }
    
    public static char[,] MakeNewCopy(ref char[,] source)
    {
        var arr = new char[source.Xdim(), source.Ydim()];
        foreach (var (x, y) in source.CoordsIterator())
        {
            arr[x, y] = source[x, y];
        }

        return arr;
    }
    public static IEnumerable<(byte,byte,char)> AsEnumerable(char[,] source)
    {
        for (byte i = 0; i < source.GetLength(0); i++)
        {
            for (byte k = 0; k < source.GetLength(1); k++)
            {
                yield return (i, k, source[i, k]);
            }
        }
    }
    
    public static void Iterate(ref char[,] source, Action<byte,byte,char> method)
    {
        for (byte i = 0; i < source.GetLength(0); i++)
        {
            for (byte k = 0; k < source.GetLength(1); k++)
            {
                method(i, k, source[i, k]);
            }
        }
    }
    public static void Replace(char[,] source, char find, char replace)
    {
        for (int i = 0; i < source.GetLength(0); i++)
        {
            for (int k = 0; k < source.GetLength(1); k++)
            {
                if (source[i, k] == find)
                    source[i, k] = replace;
            }
        }
    }
    
    public static void ReplaceAllWithOne(ref char[,] source, char[] find, char replace)
    {
        for (int i = 0; i < source.GetLength(0); i++)
        {
            for (int k = 0; k < source.GetLength(1); k++)
            {
                if (find.Contains(source[i,k]))
                    source[i, k] = replace;
            }
        }
    }
    
    public static (string head, string tail) Decap(string src, int headCount)
    {
        return (src[..headCount], src[headCount..]);
    }

    public static char[,] To2DArray(string[] input)
    {
        var output = new char[input[0].Length, input.Length];
        for (byte y = 0; y < input.Length; y++)
        {
            for (byte x = 0; x < input[y].Length; x++)
            {
                output[x, y] = input[y][x];
            }
        }
        return output;
    }
    public static string[] MakeSymmetry(string[] input, bool lowerRight)
    {
        var output = new string[input.Length];
        for (byte i = 0; i < input.Length; i++)
        {
            var r = lowerRight ? input[i].ToLower() : input[i];
            var s = new StringBuilder();
            s.Append(input[i]).Append("_").Append(Reverse(r));
            output[i] = s.ToString();
        }
        return output;
    }
    
    public static string Reverse( string s )
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}