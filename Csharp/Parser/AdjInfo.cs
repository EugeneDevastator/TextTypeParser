public struct AdjInfo
{
    public sbyte spread;
    public byte chars;
    public char a;
    public char b;

    public AdjInfo(char single)
    {
        a = single;
        chars = 1;
        spread = -1;
        b = '\0';
    }
        
    public AdjInfo(char f, char s, sbyte range)
    {
        a = f;
        b = s;
        chars = 2;
        spread = range;
    }

    public override string ToString()
    {
        return spread.ToString() + ":" + a + b;
    }
}