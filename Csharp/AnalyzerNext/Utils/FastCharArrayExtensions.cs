namespace AnalyzerUtils;

public static class FastCharArrayExtensions
{
    public static byte Xdim(this char[,] arr) => (byte)arr.GetLength(0);
    public static byte Ydim(this char[,] arr) => (byte)arr.GetLength(1);

    public static IEnumerable<(byte x, byte y)> CoordsIterator(this char[,] arr)
    {
        for (byte i = 0; i < arr.Xdim(); i++)
        {
            for (byte k = 0; k < arr.Ydim(); k++)
            {
                yield return (i, k);
            }
        }
    }
}