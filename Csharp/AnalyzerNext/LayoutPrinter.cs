using System.Text;
using AnalyzerNext;

namespace AnalyzerUtils;

public class LayoutPrinter
{
    private SymbolMap _symbolMap;

    public LayoutPrinter(SymbolMap symbolMap)
    {
        _symbolMap = symbolMap;
    }
    
    [STAThread]
    public void PrintForTable(char[,] layout)
    {
        var h = layout.GetLength(1);
        var w = layout.GetLength(0);
        StringBuilder stringBuilder = new();
        for (int k = 0; k < h; k++)
        {
            string line = "";
            for (int i = 0; i < w; i++)
            {
                var key = layout[i, k];
                if (key == '*')
                    key = '_';
                line += _symbolMap.NameOfKey(key) + " ";
                stringBuilder.Append(_symbolMap.NameOfKey(key)).Append("\t");
            }

            stringBuilder.Append("\n");
            Console.WriteLine(line);
        }
        Thread thread = new Thread(() => Clipboard.SetText(stringBuilder.ToString()));
        thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
        thread.Start(); 
        thread.Join();

        Console.WriteLine("COPIED TO CLIPBOARD");
    }
    
public void PrintRaw(CharMatrix layout) => PrintRaw(layout.Data);
    public void PrintRaw(char[,] layout)
    {
        var h = layout.GetLength(1);
        var w = layout.GetLength(0);
        StringBuilder stringBuilder = new();
        for (int k = 0; k < h; k++)
        {
            string line = "";
            for (int i = 0; i < w; i++)
            {
                var key = layout[i, k];
                line += _symbolMap.NameOfKey(key) + " ";
                stringBuilder.Append(_symbolMap.NameOfKey(key)).Append("\t");
            }

            stringBuilder.Append("\n");
            Console.WriteLine(line);
        }
        Thread thread = new Thread(() => Clipboard.SetText(stringBuilder.ToString()));
        thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
        thread.Start(); 
        thread.Join();

        Console.WriteLine("COPIED TO CLIPBOARD");
    }

    public void PrintForTable(CharMatrix bestLayout) => PrintForTable(bestLayout.Data);

}