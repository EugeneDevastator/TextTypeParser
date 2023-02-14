using System.Drawing.Text;
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

    private (float w, string pairs)[] pairSets = new[]
    {
        (0.8f, $"mc,ib,iI"),
        (1f, $"rR,mM,in,pP"),
        (1.5f, $"rd,ba,IN,nN,RP,na,In,ia"), //rd is a shitty move anyway.
        (3f, $"pY,Mc,Ib,Ia,Nb,Na,iN,pQ,QY,PY,Pd,PG,dI"),
        (4f, $"Rc,Gp,Gd,Gm,RG,Gr,Rp,Rd"),
    };

    public Dictionary<KeyCoord, List<KeyCoord>> WeightedTargets = new Dictionary<KeyCoord, List<KeyCoord>>(); 
    
    public LayoutWeights()
    {
        Dictionary<char, (byte, byte)> charToKeyPos = new Dictionary<char, (byte, byte)>();

        var halfLayout = To2DArray(_keyPlacesLeftside);
        var weightedPairs = new List<(char a, char b, float w)>();
        
        // fill coordinate lookup
        for (byte x = 0; x < halfLayout.GetLength(0); x++)
        {
            for (byte y = 0; y < halfLayout.GetLength(1); y++)
            {
                var key = halfLayout[x, y];
                charToKeyPos.Add(key, (x, y));
            }
        }
        byte fullWidth = (byte)(halfLayout.GetLength(0) * 2 + 1);
        KeyCoord GetMirroredCoord(KeyCoord src) => new KeyCoord((byte)(fullWidth - src.x - 1), src.y, src.w);


        //create double sided pairs for half layout
        foreach (var set in pairSets)
        {
            foreach (var pairChars in set.pairs.Split($","))
            {
                weightedPairs.Add((pairChars[0], pairChars[1], set.w));
                weightedPairs.Add((pairChars[1], pairChars[0], set.w)); //mirror pair
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
                    .ForEach(p =>
                    {
                        sourceList.Add(new KeyCoord(charToKeyPos[p.b], p.w));
                    });
            }
        }

        var oriKeys = WeightedTargets.Keys.ToArray();
        foreach (var pos in oriKeys)
        {
            var pMir = GetMirroredCoord(pos);
            WeightedTargets.Add(pMir,new List<KeyCoord>());
            WeightedTargets[pMir]
                .AddRange(WeightedTargets[pos]
                    .Select(GetMirroredCoord));
        }

        int a = 1;
    }
}