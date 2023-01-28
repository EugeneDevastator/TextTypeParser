using System.Text;
using AnalyzerUtils;
using Microsoft.VisualBasic.CompilerServices;

namespace AnalyzerNext;

public class CharArray
{
    private char[,] _data= new char[0, 0];
    public  char[,] Data => _data;
    public CharArray(string[] stringData)
    {
        ConvertFromStringList(stringData);
    }

    public CharArray(CharArray other)
    {
        _data = new char[other.Xdim, other.Ydim];
        foreach (var (x,y) in CoordsIterator)
        {
            _data[x, y] = other[x, y];
        }
    }

    public char this[int x, int y]
    {
        get { return _data[x, y]; }
        set { _data[x, y] = value; }
    }
    
    public char this[KeyCoord c]
    {
        get { return _data[c.x, c.y]; }
        set { _data[c.x, c.y] = value; }
    }
    
    public char this[(byte x, byte y) c]
    {
        get { return _data[c.x, c.y]; }
        set { _data[c.x, c.y] = value; }
    }
    
    public byte Xdim => (byte)_data.GetLength(0);
    public byte Ydim => (byte)_data.GetLength(1);
    private void ConvertFromStringList(string[] input)
    {
        _data = new char[input[0].Length, input.Length];
        for (byte y = 0; y < input.Length; y++)
        {
            for (byte x = 0; x < input[y].Length; x++)
            {
                _data[x, y] = input[y][x];
            }
        }
    }

    public IEnumerable<(byte x, byte y)> CoordsIterator
    {
        get
        {
            for (byte i = 0; i < Xdim; i++)
            {
                for (byte k = 0; k < Ydim; k++)
                {
                    yield return (i, k);
                }
            }
        }
    }

    public IEnumerable<char> Flatten => CoordsIterator.Select(pos => _data[pos.x, pos.y]);
    
    public void UpdateEach(Func<char, char> updater)
    {
        foreach (var (x,y) in CoordsIterator)
        {
            _data[x, y] = updater(_data[x, y]);
        }
    }
    
    public void ReplaceAllWithOne(char[] find, char replace)
    {
        UpdateEach(c=> find.Contains(c) ? replace : c);
    }

    public void ReplaceAllWithOne(char find, char replace)
    {
        UpdateEach(c=> c==find ? replace : c);
    }
    
    public override string ToString()
    {
        return new string(ToStringList().SelectMany(e => (e + "\n").ToCharArray()).ToArray());
    }

    public List<string> ToStringList()
    {
        var output = new List<string>();
        StringBuilder stringBuilder = new();
        for (int k = 0; k < Ydim; k++)
        {
            string line = "";
            for (int i = 0; i < Xdim; i++)
            {
                var key = _data[i, k];
                stringBuilder.Append(key);
            }
            output.Add(stringBuilder.ToString());
            stringBuilder.Clear();
        }

        return output;
    }
    
    
    
}