using System.Text;
using AnalyzerUtils;

namespace AnalyzerNext;

public class Sturct2D<T> where T : struct, IEquatable<T>
{
    private T[,] _data= new T[0, 0];
    private List<(byte x, byte y)> _flatCoords = new List<(byte x, byte y)>();
    public  T[,] Data => _data;

    public Sturct2D(Sturct2D<T> other)
    {
        _data = new T[other.Xdim, other.Ydim];

        RegenerateCoords();
        
        foreach (var (x,y) in CoordsList)
        {
            _data[x, y] = other[x, y];
        }
    }

    private void RegenerateCoords()
    {
        for (byte i = 0; i < Xdim; i++)
        {
            for (byte k = 0; k < Ydim; k++)
            {
                _flatCoords.Add((i, k));
            }
        }
    }

    public void CopyDataFrom(Sturct2D<T> other)
    {
        //_data = new char[other.Xdim, other.Ydim];
        foreach (var (x,y) in CoordsList)
        {
            _data[x, y] = other[x, y];
        }
    }
    
    public T this[int x, int y]
    {
        get => _data[x, y];
        set => _data[x, y] = value;
    }
    public T this[KeyCoord c]      
    {
        get => this[c.x, c.y];
        set => this[c.x, c.y] = value;
    }
    public T this[(byte x, byte y) c]     
    {
        get => this[c.x, c.y];
        set => this[c.x, c.y] = value;
    }

    public byte Xdim => (byte)_data.GetLength(0);
    public byte Ydim => (byte)_data.GetLength(1);
    
    public List<(byte x, byte y)> CoordsList
    {
        get => _flatCoords;
    }

    public List<T> Flatten => CoordsList.Select(pos => this[pos]).ToList();
    
    public void UpdateEach(Func<T, T> updater)
    {
        foreach (var (x,y) in CoordsList)
        {
            _data[x, y] = updater(_data[x, y]);
        }
    }
    
    public void ReplaceManyWithOne(T[] find, T replace)
    {
        UpdateEach(c=> find.Contains(c) ? replace : c);
    }

    public void ReplaceAllWithOne(T find, T replace)
    {
        UpdateEach(c=> c.Equals(find) ? replace : c);
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