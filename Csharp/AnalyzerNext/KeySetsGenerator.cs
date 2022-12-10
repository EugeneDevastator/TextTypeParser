using System.Net.Security;
using System.Security.Cryptography.Xml;
using Combinatorics.Collections;
using NumSharp.Utilities;
using static AnalyzerNext.Utils;

namespace AnalyzerNext;

public class KeySetsGenerator
{
    private IDataContainer _data;
    // 1
    // 2
    // staged
    private LayoutConfig _layout;

    
    //unstaged
    private KeySetSampler _sampler;
    // find lowest score combinations for each, thinking that index finger can handle 4 keys.
// and find lowest score for all.

// places per finger = 33333344
// foreach perm in places.


    public KeySetsGenerator(LayoutConfig layout, IDataContainer data)
    {
        _layout = layout;
        _data = data;
        _sampler = new KeySetSampler(data);
        // get main keys
        // get best first complement, it may not end up being best but will be good enough and save time.
        // get best second complement
        // find best two fingers for 4 keys
        // arange those sets on a keyboard to avoid shit combos


        //keys per finger perms
        string a = "33333344";
        var p = new Permutations<char>(a.ToCharArray(), GenerateOption.WithoutRepetition);
        Console.WriteLine(p.Count);
        foreach (var perm in p)
        {
            Console.WriteLine(string.Join("", perm));
        }

        //for each of such combo get permutations of remaining keys..
    }

    public void GenerateLayout()
    {
        var FINGERS = 8;
        var mainKeyset = new LogicalKeySet();

        var skipKeys = _layout.SkippedKeys;
        var orderedKeys =
            new string(_data.KeyCounts
                .Select((count, i) => (count, i))
                .OrderByDescending(e => e.count)
                .Select(e => _data.Keys[e.i])
                .ToArray());
        var keysRemain = Expel(orderedKeys, skipKeys);
        string sampleKeys = "";
        
        //MAINS
        (sampleKeys, keysRemain) = Decap(keysRemain,FINGERS);
        sampleKeys = FillToCount(sampleKeys, FINGERS);
        mainKeyset.Main = sampleKeys[..FINGERS].ToCharArray();
        Console.WriteLine(sampleKeys);
        
        //FIRST
        (sampleKeys, keysRemain) = Decap(keysRemain,FINGERS);
        sampleKeys = FillToCount(sampleKeys, FINGERS);
        Console.WriteLine(sampleKeys);
        float score;        

        var bestScore = float.MaxValue;

        var cacheSet = mainKeyset;
        var permutations = new Variations<char>(sampleKeys.ToCharArray(), FINGERS);
        Console.WriteLine(permutations.Count);
        foreach (var perm in permutations)
        {
            cacheSet.FirstComplement = perm.ToArray();
            score = _sampler.GetTotalScore(ref cacheSet);
            if (score < bestScore)
            {
                mainKeyset = cacheSet;
                bestScore = score;
            }
        }
        
        // SECOND
        
        (sampleKeys, keysRemain) = Decap(keysRemain,FINGERS - 4);
        sampleKeys = FillToCount(sampleKeys, FINGERS);
        cacheSet = mainKeyset;
        bestScore = float.MaxValue;
        var placePairs = new Variations<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 2, GenerateOption.WithoutRepetition);
        var count = placePairs.Count;
        byte[] bestPlaces = new byte[] { };
        permutations = new Variations<char>(sampleKeys.ToCharArray(), FINGERS);
        foreach (var perm in permutations)
        {
            cacheSet.SecondComplement = perm.ToArray();

            foreach (var place in placePairs)
            {

                score = _sampler.GetTotalScore(ref cacheSet, place.ToArray());
                if (score < bestScore)
                {
                    mainKeyset = cacheSet;
                    bestScore = score;
                    bestPlaces = place.ToArray();
                }
            }
        }
        
        //REMAINDER.
        if (true)
        {
            keysRemain = FillToCount(keysRemain, FINGERS);
            (sampleKeys, keysRemain) = Decap(keysRemain, FINGERS);
            //sampleKeys = FillToCount(sampleKeys, FINGERS);
            cacheSet = mainKeyset;
            bestScore = float.MaxValue;

            permutations = new Variations<char>(sampleKeys.ToCharArray(), FINGERS);
            foreach (var perm in permutations)
            {
                cacheSet.Remainder = perm.ToArray();
                score = _sampler.GetTotalScore(ref cacheSet, bestPlaces);
                if (score < bestScore)
                {
                    mainKeyset = cacheSet;
                    bestScore = score;
                }
            }
        }
        
        //var all = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        //all.RemoveAt(bestPlaces[0]);
        //all.RemoveAt(bestPlaces[1]);
//
        //var orderPerms = new Permutations<byte>(all, GenerateOption.WithoutRepetition);
        //var bestOrder = all;
        //foreach (var perm in orderPerms)
        //{
        //    
        //}

        var scores = Enumerable.Range(0, mainKeyset.Fingers)
            .Select(i => (i,_sampler.SampleFinger((byte)i, ref mainKeyset, ref bestPlaces))).ToArray();
        var topMiddles = scores
            .Where(s=>mainKeyset.SecondComplement[(byte)s.i] != '_')
            .OrderByDescending(s => s.Item2)
            .Select(s=>s.i).ToArray()[..2];
        var topTwoMain = 
            scores.Where(s=>!bestPlaces.Contains((byte)s.i))
                .Where(s=>mainKeyset.SecondComplement[(byte)s.i] == '_')
                .OrderByDescending(s => 
                    _sampler.GetFingerMainCount(s.i,ref mainKeyset))
                .Select(s=>s.i)
                .ToArray()[..2];
        
        var lowTwo = scores.OrderBy(s => s.Item2).Select(s=>s.i).ToArray()[..2];
        
        Console.WriteLine("\n");
        for (byte i = 0; i < mainKeyset.Fingers; i++)
        {
            bool star = bestPlaces.Contains(i);
            bool middle = topMiddles.Contains(i);
            bool pinky = lowTwo.Contains(i);
            bool ring = topTwoMain.Contains(i);
            Console.WriteLine(mainKeyset.Main[i].ToString()
                              +mainKeyset.FirstComplement[i].ToString()
                              +mainKeyset.SecondComplement[i].ToString()
                              +mainKeyset.Remainder[i].ToString()
                              +" : "
                              +_sampler.SampleFinger(i,ref mainKeyset,ref bestPlaces)
                              + (star ? "[*idx]" : "")
                              + (ring ? "<R>" : "")
                              + (middle ? "(M)" : "")
                              + (pinky ? ".p." : "")
                              );
        }
        Console.WriteLine(keysRemain);
        //Console.WriteLine(mainKeyset.ToString());
        foreach (var p in bestPlaces)
        {
            Console.Write(p+",");    
        }
        
        
        //var permutations = new Permutations<char>(sampleKeys.ToCharArray());


        //string usedKeys = "";
        //if (bestKeySet == null)
        //{
        //    usedKeys = sampleKeys[..posSet.Count];
        //}
        //else
        //{
        //    usedKeys = string.Join("", bestKeySet)[..posSet.Count];
        //}

        //keysRemain = Expel(keysRemain, usedKeys);
        // make variations for available places
        // var v = new Variations(keys, number of places);
        //
        //
    }
}