namespace AnalyzerNext;

public class Sampler
{
    private IDataContainer _data;
    private LayoutData _layout;

    public Sampler(IDataContainer data, LayoutData layout)
    {
        _layout = layout;
        _data = data;
    }

    public IEnumerable<List<(byte x, byte y)>> PriorityPositions()
    {
        var keysDesc = _layout.PriorityKeysAscending.Reverse();
        foreach (var pKey in keysDesc)
        {
            var res = new List<(byte x, byte y)>();
            for (byte i = 0; i < _layout.XDim; i++)
            {
                for (byte k = 0; k < _layout.YDim; k++)
                {
                    if (_layout.FixedKeys[i, k] != LayoutData.SKIP && _layout.Priorities[i, k] == pKey)
                        res.Add((i, k));
                }
            }

            if (res.Count > 0)
                yield return res;
        }
    }

    public float GetWeightOfPlacedKey(byte x, byte y, char key, ref char[,] filledLayout)
    {
        return _data.GetAdjMetric(filledLayout[x, y], key);
    }

    public float GetWeight(byte x, byte y, byte x1, byte y1, byte dist, ref char[,] filledLayout) =>
        GetWeight((x, y), (x1, y1), dist, ref filledLayout);

    public float GetWeight((byte x, byte y) pos1, (byte x, byte y) pos2, byte dist, ref char[,] filledLayout)
    {
        return _data.GetAdjMetric(filledLayout[pos1.x, pos1.y], filledLayout[pos2.x, pos2.y]);
    }

    private List<(byte x, byte y, byte dist)> GetCompetingPositions(byte x, byte y)
    {
        var res = new List<(byte, byte, byte)>();
        var competeKey = _layout.FingerGroups[x, y];
        for (int i = -1; i <= 1; i++)
        {
            for (int k = -1; k <= 1; k++)
            {
                var Xs = x + i;
                var Ys = y + k;
                if (IsValid(Xs,Ys) && i != 0 && k != 0 && _layout.FingerGroups[Xs,Ys]==competeKey)
                {
                    res.Add(((byte, byte, byte))(Xs,Ys, Math.Max(Math.Abs(i), Math.Abs(k))));
                }
            }
        }

        return res;
    }

    bool IsValid(int x, int y) =>
        x >= 0
        && x < _layout.XDim
        && y >= 0
        && y < _layout.YDim
        && _layout.FixedKeys[x, y] != LayoutData.SKIP;

    //bool IsValidChar(int x, int y) =>
    //    x >= 0 && x < w && y >= 0 && y < h && chars[x, y] != SKIP && chars[x, y] != NONE;
//
    //bool IsValidEmpty(int x, int y) =>
    //    x >= 0 && x < w && y >= 0 && y < h && chars[x, y] != SKIP && chars[x, y] == NONE;

    //Assume Keypos is valid
    public float SampleKey(ref char[,] filledLayout, (byte x, byte y) keyPos)
    {
        float result = 0;
        foreach (var pos in GetCompetingPositions(keyPos.x, keyPos.y))
        {
            if (filledLayout[pos.x, pos.y] != LayoutData.SKIP && filledLayout[pos.x, pos.y] != LayoutData.NONE)
                result += GetWeight(pos.x, pos.y, keyPos.x, keyPos.y, pos.dist, ref filledLayout);
        }

        return result;
    }
}