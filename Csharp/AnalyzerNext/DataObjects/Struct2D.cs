using System.Text;
using AnalyzerUtils;

namespace AnalyzerNext;

public class Struct2D<T> where T : struct, IEquatable<T>
{
    private readonly T[,] _data;
    private readonly (ushort x, ushort y)[] _flatCoords;


    public Struct2D(ushort width, ushort height)
    {
        _data = new T[width, height];
        _flatCoords = GenerateCoords().ToArray();
    }

    public Struct2D(Struct2D<T> other) : this(other.Xdim, other.Ydim)
    {
        foreach (var (x, y) in _flatCoords)
            _data[x, y] = other[x, y];
    }

    public T[,] Data => _data;
    public ushort Xdim => (ushort)_data.GetLength(0);
    public ushort Ydim => (ushort)_data.GetLength(1);
    public (ushort x, ushort y)[] CoordsArray => _flatCoords;
    public List<T> Flatten => CoordsArray.Select(pos => this[pos]).ToList();

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

    public T this[(ushort x, ushort y) c]     
    {
        get => this[c.x, c.y];
        set => this[c.x, c.y] = value;
    }

    public static Struct2D<T> Map<TRead>((ushort x, ushort y) end,Func<int,int,TRead> getter, Func<TRead,T> mapper)
    {
        return Map((0, 0), end, getter, mapper);
    }

    public static Struct2D<T> Map<TRead>((ushort x, ushort y) start, (ushort x, ushort y) end, Func<int, int, TRead> getter, Func<TRead, T> mapper)
    {
        (ushort x, ushort y) dims = ((ushort)(end.x - start.x + 1), (ushort)(end.y - start.y + 1));
        var s2d = new Struct2D<T>(dims.x, dims.y);
        for (ushort i = 0; i < dims.x; i++)
        {
            for (ushort k = 0; k < dims.y; k++)
            {
                s2d[i, k] = mapper(getter(start.x + i, start.y + k));
            }
        }
        return s2d;
    }

    private IEnumerable<(ushort x, ushort y)> GenerateCoords()
    {
        for (ushort i = 0; i < Xdim; i++)
        {
            for (ushort k = 0; k < Ydim; k++)
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