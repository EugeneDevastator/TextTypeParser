using System.Text;
using AnalyzerUtils;

namespace AnalyzerNext;

public class Struct2D<T> where T : struct, IEquatable<T>
{
    private readonly T[,] _data;
    private readonly (byte x, byte y)[] _flatCoords;

    public T[,] Data => _data;
    public byte Xdim => (byte)_data.GetLength(0);
    public byte Ydim => (byte)_data.GetLength(1);
    public (byte x, byte y)[] CoordsArray => _flatCoords;
    public List<T> Flatten => CoordsArray.Select(pos => this[pos]).ToList();


    public Struct2D(int width, int height)
    {
        _data = new T[width, height];
        _flatCoords = GenerateCoords().ToArray();
    }

    public Struct2D(Struct2D<T> other) : this(other.Xdim, other.Ydim)
    {
        foreach (var (x, y) in _flatCoords)
            _data[x, y] = other[x, y];
    }

    private IEnumerable<(byte x, byte y)> GenerateCoords()
    {
        for (byte i = 0; i < Xdim; i++)
        {
            for (byte k = 0; k < Ydim; k++)
            {
                yield return (i, k);
            }
        }
    }

    public void CopyDataFrom(Struct2D<T> other)
    {
        //_data = new char[other.Xdim, other.Ydim];
        foreach (var (x,y) in CoordsArray)
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
    
    public void UpdateEach(Func<T, T> updater)
    {
        foreach (var (x,y) in CoordsArray)
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