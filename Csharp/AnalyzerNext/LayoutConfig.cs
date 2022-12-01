using System.Text;

namespace AnalyzerNext;

public class LayoutConfig
{
    public const char SKIP = '_';
    public const char NONE = '*';
    
    public string SkippedKeys = " \\-90[]/'=;.,";
    public int AddToSample = 0;
    
    private string[] _fixedKeys = new string[]
    {
        //"_*****_*****_",
        //"_*rst*_*nei*_",
        //"*a****_****o*",
        "******_******",
        "***s**_**ie**",
        "*a*c**_****o*",
    };
    
    public string PriorityKeysAscending = "0123456789ABCDEF";
    private string[] _halfPriority = new string[]
    {
        "238833",
        "39FDC9",
        "8D9998",
    };

    public string FingerChars = "ABCDabcd";
    private string[] _fingerGroups = new string[]
    {
        "CCCBAA",
        "DDCBAA",
        "DDCBAA",
    };

    public char[,] FingerGroups;
    public char[,] Priorities;
    public char[,] FixedKeys;
    
    public int XDim => FixedKeys.GetLength(0);
    public int YDim => FixedKeys.GetLength(1);
    
    public LayoutConfig()
    {
        FingerGroups = To2DArray(MakeSymmetry(_fingerGroups, true));
        Priorities = To2DArray(MakeSymmetry(_halfPriority, false));
        FixedKeys = To2DArray(_fixedKeys);
    }

    char[,] To2DArray(string[] input)
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
    private string[] MakeSymmetry(string[] input, bool lowerRight)
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