namespace AnalyzerUtils;

public struct KeyCoord
{
    public byte x;
    public byte y;
    public float w = 1f;

    public KeyCoord(byte x, byte y, float w = 1f)
    {
        this.x = x;
        this.y = y;
        this.w = w;
    }

    public KeyCoord((byte x, byte y) t, float w = 1f)
    {
        this.x = t.x;
        this.y = t.y;
        this.w = w;
    }
    public KeyCoord((int x, int y) t, float w = 1f)
    {
        this.x = (byte)t.x;
        this.y = (byte)t.y;
        this.w = w;
    }
    public static implicit operator KeyCoord((byte x, byte y) t) => new KeyCoord(t);
    public static implicit operator KeyCoord((int x, int y) t) => new KeyCoord(t);

    public override string ToString()
    {
        return $"{x},{y} : {w}";
    }
}