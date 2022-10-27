﻿using System.Text;

public class WordSplitter
{
    private SymbolMap _symbols;
    private const string CrumbSeparators = ".;_ ";
    private const string Ignore = "\n\r\t";

    public WordSplitter(SymbolMap symbols)
    {
        _symbols = symbols;
    }

    public IEnumerable<string> WordsOf(string content, bool addSeparators)
    {
        StringBuilder wordBuilder = new StringBuilder();
        foreach (var cr in content)
        {
            if (!_symbols.Letters.Contains(cr))
            {
                if (addSeparators && !Ignore.Contains(cr))
                    wordBuilder.Append(cr);

                yield return wordBuilder.ToString();
                wordBuilder.Clear();
                continue;
            }

            wordBuilder.Append(cr);
        }
    }

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

    public IEnumerable<AdjInfo> AdjOf(string word)
    {
        char a = '\0';
        char b = '\0';
        char c = '\0';
        for (int i = 0; i < word.Length; i++)
        {
            yield return new AdjInfo(word[i]);
            if (i > 0)
                yield return new AdjInfo(word[i-1], word[i],0);
            if (i > 1)
                yield return new AdjInfo(word[i-2], word[i],1);
        }
    }
    public IEnumerable<string> IntelCrumbsOf(string word, int nchars)
    {
        StringBuilder wordBuilder = new StringBuilder();
        byte charN = 0;
        bool skipToSep = false;
        char lastChar = '\0';
        foreach (var cr in word)
        {
            bool isSep = (_symbols.LowerLetters.Contains(lastChar) && _symbols.UpperLetters.Contains(cr))
                         || !_symbols.Letters.Contains(cr);

            if (skipToSep && !isSep)
            {
                lastChar = cr;
                continue;
            }
            skipToSep = false;
            
            if (charN != 0 && isSep)
            {
                if (CrumbSeparators.Contains(cr))
                    wordBuilder.Append(cr);

                yield return wordBuilder.ToString();
                wordBuilder.Clear();
                charN = 0;

                if (_symbols.UpperLetters.Contains(cr))
                {
                    wordBuilder.Append(cr);
                    skipToSep = false;
                }
                else 
                    skipToSep = true;

                continue;
            }
            
            wordBuilder.Append(cr);
            charN++;
            lastChar = cr;
            
            if (charN >= nchars)
            {
                yield return wordBuilder.ToString();
                wordBuilder.Clear();
                skipToSep = true;
                charN = 0;
            }
        }

        if (wordBuilder.Length > 0)
            yield return wordBuilder.ToString();
    }
}