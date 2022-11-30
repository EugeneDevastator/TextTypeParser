namespace AnalyzerNext;

public class LayoutPrinter
{
    private SymbolMap _symbolMap;

    public LayoutPrinter(SymbolMap symbolMap)
    {
        _symbolMap = symbolMap;
    }

    public void PrintForTable(char[,] layout)
    {
        var h = layout.GetLength(1);
        var w = layout.GetLength(0);
        for (int k = 0; k < h; k++)
        {
            string line = "";
            for (int i = 0; i < w; i++)
            {
                var key = layout[i, k];
                if (key == '*')
                    key = '_';
                line += _symbolMap.NameOfKey(key) + " ";
            }

            Console.WriteLine(line);
        }
        
    }

    //void PrintRaw()
    //{
    //    for (int k = 0; k < h; k++)
    //    {
    //        string line = "";
    //        for (int i = 0; i < w; i++)
    //        {
    //            line += chars[i, k];
    //        }

    //        Console.WriteLine(line);
    //    }
    //}
}