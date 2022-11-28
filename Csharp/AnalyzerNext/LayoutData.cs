using System.Text;

namespace AnalyzerNext;

public class LayoutData
{
    public const char SKIP = '_';
    public const char NONE = '*';

    private string[] _fixedKeys = new string[]
    {
        "_;***___*.,*_",
        "_urst*_*nei=_",
        "*a****_***yo*",
    };

    public int XDim => FixedKeys.GetLength(0);
    public int YDim => FixedKeys.GetLength(1);
    
    public string PriorityKeysAscending = "0123456789ABCDEF";

    private string[] _halfPriority = new string[]
    {
        "016661",
        "08ABA7",
        "1A8887",
    };
    
    private string[] _fingerGroups = new string[]
    {
        "CCCBAA",
        "DDCBAA",
        "DDCBAA",
    };

    public char[,] FingerGroups;
    public char[,] Priorities;
    public char[,] FixedKeys;
    
    
    public LayoutData()
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