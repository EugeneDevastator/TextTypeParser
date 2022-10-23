using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CharData;

public class KeyData
{
    public string typable; // = "abcdefghijklmnopqrstuvwxyz .,;"; //stored data.

    public Dictionary<char, string> charNames = new Dictionary<char, string>()
    {
        { ' ', "spc" },
        { '.', "dot" },
        { ',', "coma" },
        { ';', "semi" },
        { '\'', "quot" },
        { '\\', "slsh" },
        { '-', "minus" },
        { '[', "lbr" },
        { ']', "rbr" },
        { '*', "non" },
    };

    public string[] typableNames;

    public IReadOnlyDictionary<char, byte> TypIndices => typIndices;
    private Dictionary<char, byte> typIndices = new Dictionary<char, byte>();
    
    public KeyData(string uniqueKeys)
    {
        typable = uniqueKeys;
        typableNames = new string[typable.Length];

        for (int i = 0; i < typable.Length; i++)
        {
            typableNames[i] = charNames.ContainsKey(typable[i]) ? charNames[typable[i]] : typable[i].ToString();
            typIndices[typable[i]] = (byte)i;
        }
    }

    public string NameOf(char c) => typIndices.ContainsKey(c) ? typableNames[typIndices[c]] : c.ToString();
    public int IdxOf(char c) => typIndices[c];
    public int CharAt(int id) => typable[id];
}

public class CharIndexedData<T>
{
    private readonly Array _data;
    private string _orderedChars;

    public CharIndexedData(Array data,  string orderedChars)
    {
        _data = data;
        _orderedChars = orderedChars;
    }
}
