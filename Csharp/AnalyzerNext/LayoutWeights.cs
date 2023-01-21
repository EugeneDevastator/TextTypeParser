using AnalyzerUtils;
using MoreLinq.Extensions;
using static AnalyzerUtils.Utils;
namespace AnalyzerNext;

public class LayoutWeights
{
    private string[] _keyPlacesLeftside = new string[]
    {
        "_GRMIN",
        "QPrmin",
        "Ypdcba",
    };

    private string pairsEasy = $"rd,mc,ib,rR,mM,iI,in,pP,Nn,na";
    private string pairsMedium = $"ia,pY,ba,NI";
    private string pairsHard = $"Rd,Mc,Ib,Nb,Na,iN,pQ,QY,PY,Pd,da";
    private string pairsVeryhard = $"Rc,Gp,Gd,Gm";
    
    private float pairsEasyW = 1f;
    private float pairsMediumW = 1.5f;
    private float pairsHardW = 2f;
    private float pairsVeryhardW = 4f;
    public Dictionary<KeyCoord, List<KeyCoord>> WeightedTargets = new Dictionary<KeyCoord, List<KeyCoord>>(); 
    
    public LayoutWeights()
    {
        Dictionary<char, (byte, byte)> charToKeyPos = new Dictionary<char, (byte, byte)>();

        var halfLayout = To2DArray(_keyPlacesLeftside);
        var weightedPairs = new List<(char a, char b, float w)>();

        foreach (var s in pairsEasy.Split($","))
            weightedPairs.Add((s[0], s[1], pairsEasyW));
        foreach (var s in pairsMedium.Split($","))
            weightedPairs.Add((s[0], s[1], pairsMediumW));
        foreach (var s in pairsHard.Split($","))
            weightedPairs.Add((s[0], s[1], pairsHardW));
        foreach (var s in pairsVeryhard.Split($","))
            weightedPairs.Add((s[0], s[1], pairsVeryhardW));

        //reverse copy
        for (var i = weightedPairs.Count - 1; i >= 0; i--)
        {
            var p = weightedPairs[i];
            weightedPairs.Add((p.b, p.a, p.w));
        }

        for (byte x = 0; x < halfLayout.GetLength(0); x++)
        {
            for (byte y = 0; y < halfLayout.GetLength(1); y++)
            {
                var key = halfLayout[x, y];
                charToKeyPos.Add(key, (x, y));
            }
        }
// fill cords and weights
        for (byte x = 0; x < halfLayout.GetLength(0); x++)
        {
            for (byte y = 0; y < halfLayout.GetLength(1); y++)
            {
                var key = halfLayout[x, y];
                if (!WeightedTargets.ContainsKey((x, y)))
                {
                    WeightedTargets.Add((x, y), new List<KeyCoord>());
                }

                var sourceList = WeightedTargets[(x, y)];
                weightedPairs.Where(p => p.a == key)
                    .ForEach(p => sourceList.Add(new KeyCoord(charToKeyPos[p.b], p.w)));
            }
        }

        byte width = (byte)(halfLayout.GetLength(0) * 2 + 1);
        //make reverse copy
        foreach (var pos in WeightedTargets.Keys)
        {
            var pMir = pos;
            pMir.x = (byte)(width - pMir.x);
            WeightedTargets.Add(pMir,new List<KeyCoord>());
            WeightedTargets[pMir]
                .AddRange(WeightedTargets[pos]
                    .Select(kc=>new KeyCoord((byte)(width - kc.x),kc.y,kc.w)));
        }
    }
}