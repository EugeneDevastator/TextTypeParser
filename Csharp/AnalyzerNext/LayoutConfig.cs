using System.Text;
using AnalyzerUtils;
using MoreLinq;
using MoreLinq.Extensions;
using static AnalyzerUtils.Utils;

namespace AnalyzerNext;

public class LayoutConfig
{
    public const char SKIP = '_';
    public const char NONE = '*';
    
    public string SkippedKeys = " \\-90[]/'=;.,`";
    public int AddToSample = 0;
    
    private string[] _fixedKeys = new string[]
    {
        //"_*****_*****_",
        //"_*rst*_*nei*_",
        //"*a****_****o*",
        "__****_****_-",
        "_*rst*_*nie*_",
        "_a________*o_",
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
}