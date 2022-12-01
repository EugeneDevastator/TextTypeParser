using System.Text;

namespace AnalyzerNext;

public struct LogicalKeySet
{
    public int Fingers = 8;
    public char[] Main = new char[] {'_','_','_','_','_','_','_','_'};
    public char[] FirstComplement = new char[] {'_','_','_','_','_','_','_','_'};
    public char[] SecondComplement = new char[] {'_','_','_','_','_','_','_','_'};
    public char[] Remainder = new char[] {'_','_','_','_','_','_','_','_'};

    public LogicalKeySet()
    {
    }

    public override string ToString()
    {
        StringBuilder s=new();
        for (int i = 0; i < Fingers; i++)
        {
            s.Append(Main[i]).Append(FirstComplement[i]).Append(SecondComplement[i]).Append(Remainder[i]).Append("\n");
        }

        return s.ToString();
    }
}