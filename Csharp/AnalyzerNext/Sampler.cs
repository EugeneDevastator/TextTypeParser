using System.ComponentModel;
using System.Text;
using AnalyzerNext;
using Combinatorics.Collections;

namespace AnalyzerUtils;

public class Sampler
{
    private IDataContainer _data;
    private LayoutConfig _layout;

    public Sampler(IDataContainer data, LayoutConfig layout)
    {
        _layout = layout;
        _data = data;
    }

    public IEnumerable<(byte x, byte y)> BadPairsForPos(byte x, byte y)
    {
        var pos = (x + 1, y + 2);
        if (IsValid(pos)) yield return ((byte x, byte y))pos;

        pos = (x + 1, y - 2);
        if (IsValid(pos)) yield return ((byte x, byte y))pos;

        pos = (x - 1, y - 2);
        if (IsValid(pos)) yield return ((byte x, byte y))pos;

        pos = (x - 1, y + 2);
        if (IsValid(pos)) yield return ((byte x, byte y))pos;
    }

    public IEnumerable<(byte x, byte y)[]> BadPairsInLayout()
    {
        byte maxy = (byte)(_layout.YDim - 1);
        
        for (byte i = 0; i < _layout.XDim-1; i++)
        {
            yield return new (byte x, byte y)[] { (i, maxy), ((byte)(i + 1), 0) };
            yield return new (byte x, byte y)[] { (i, 0), ((byte)(i + 1), maxy) };
        }
        
    }
    
    public IEnumerable<List<(byte x, byte y)>> FingerSetPositionsForPlacedKeys(char[,] filledLayout)
    {
        var keysDesc = _layout.FingerChars;
        foreach (var groupKey in keysDesc)
        {
            var res = new List<(byte x, byte y)>();
            for (byte i = 0; i < _layout.XDim; i++)
            {
                for (byte k = 0; k < _layout.YDim; k++)
                {
                    if (filledLayout[i, k] != LayoutConfig.SKIP
                        && filledLayout[i, k] != LayoutConfig.NONE
                        && _layout.FingerGroups[i, k] == groupKey)
                        res.Add((i, k));
                }
            }

            if (res.Count > 0)
                yield return res;
        }
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
                    if (_layout.FixedKeys[i, k] == LayoutConfig.NONE && _layout.Priorities[i, k] == pKey)
                        res.Add((i, k));
                }
            }

            if (res.Count > 0)
                yield return res;
        }
    }

    public string GetUsedKeys(char[,] filledLayout)
    {
        var xdim = filledLayout.GetLength(0);
        var ydim = filledLayout.GetLength(1);
        StringBuilder s = new();
        for (int i = 0; i < xdim; i++)
        {
            for (int k = 0; k < ydim; k++)
            {
                if (filledLayout[i, k] != LayoutConfig.SKIP && filledLayout[i, k] != LayoutConfig.NONE)
                {
                    s.Append(filledLayout[i, k]);
                }
            }
        }

        return s.ToString();
    }

    public float GetWeightOfPlacedKey(byte x, byte y, char key, ref char[,] filledLayout)
    {
        return _data.GetAdjMetric(filledLayout[x, y], key);
    }

    public float GetWeight(byte x, byte y, byte x1, byte y1, byte dist, ref char[,] filledLayout) =>
        GetWeight((x, y), (x1, y1), dist, ref filledLayout);

    public float GetWeight((byte x, byte y) pos1, (byte x, byte y) pos2, byte dist, ref char[,] filledLayout)
    {
        if (filledLayout[pos1.x, pos1.y] == LayoutConfig.SKIP || filledLayout[pos2.x, pos2.y] == LayoutConfig.SKIP)
            return 0;

        return dist * _data.GetAdjMetric(filledLayout[pos1.x, pos1.y], filledLayout[pos2.x, pos2.y]);
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
                if (IsValid(Xs, Ys) && i != 0 && k != 0 && _layout.FingerGroups[Xs, Ys] == competeKey)
                {
                    res.Add(((byte, byte, byte))(Xs, Ys, Math.Max(Math.Abs(i), Math.Abs(k))));
                }
            }
        }

        return res;
    }

    bool IsValid(byte x, byte y) =>
        x >= 0
        && x < _layout.XDim
        && y >= 0
        && y < _layout.YDim
        && _layout.FixedKeys[x, y] != LayoutConfig.SKIP;

    bool IsValid(int x, int y) => IsValid((byte)x, (byte)y);

    bool IsValid((int x, int y) p) => IsValid((byte)p.x, (byte)p.y);

    bool IsValidKey((int x, int y) p, ref char[,] filledLayout) =>
        IsValid((byte)p.x, (byte)p.y)
        && filledLayout[p.x, p.y] != LayoutConfig.NONE
        && filledLayout[p.x, p.y] != LayoutConfig.SKIP;

    //bool IsValidChar(int x, int y) =>
    //    x >= 0 && x < w && y >= 0 && y < h && chars[x, y] != SKIP && chars[x, y] != NONE;
//
    //bool IsValidEmpty(int x, int y) =>
    //    x >= 0 && x < w && y >= 0 && y < h && chars[x, y] != SKIP && chars[x, y] == NONE;

    public IEnumerable<float> GetScoresForPositionPairs(IReadOnlyList<(byte x, byte y)> inlist, char[,] filledLayout)
    {
        if (inlist.Count() < 2)
        {
            yield return 0;
            yield break;
        }

        var ids = inlist.Select((p, i) => i).ToList();
        var pairs = new Combinations<int>(ids, 2);
        foreach (var pair in pairs)
        {
            var pos1 = inlist[pair[0]];
            var pos2 = inlist[pair[1]];
            yield return GetPairScore(pos1.x, pos1.y, pos2.x, pos2.y, ref filledLayout);
        }
    }

    public float GetLayoutScoreTotal(ref char[,] filledLayout)
    {
        // for each finger-set with existing keys
        // get finger set score
        // for each unique pair
        // sum up score

        var fingerKeySets = FingerSetPositionsForPlacedKeys(filledLayout);
        //var badCombos = BadPairsInLayout().Where(p=>IsValidKey(p[0],filledLayout))
        float score = 0;
        foreach (var posSet in fingerKeySets)
        {
            score += GetScoresForPositionPairs(posSet,filledLayout).Sum();
        }

        return score;
    }

    public float GetLayoutScoreMax(ref char[,] filledLayout)
    {
        // for each finger-set with existing keys
        // get finger set score
        // for each unique pair
        // sum up score

        var lists = FingerSetPositionsForPlacedKeys(filledLayout);
        float score = 0;
        foreach (var posSet in lists)
        {
            var ids = posSet.Select((p, i) => i).ToList();
            var pairs = new Combinations<int>(ids, 2);
            foreach (var pair in pairs)
            {
                var pos1 = posSet[pair[0]];
                var pos2 = posSet[pair[1]];
                var pairS = GetPairScore(pos1.x, pos1.y, pos2.x, pos2.y, ref filledLayout);
                score = Math.Max(score, pairS);
            }
        }

        return score;
    }

    public float GetPairScore(byte x1, byte y1, byte x2, byte y2, ref char[,] layout)
    {
        byte dist = (byte)Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        var s = GetWeight(x1, y1, x2, y2, dist, ref layout);
        return s;
    }

    //we in fact must sample each unique pair in each finger set.
}