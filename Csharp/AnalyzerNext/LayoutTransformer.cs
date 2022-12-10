using System.Text;

namespace AnalyzerNext;

public class LayoutTransformer
{
    public static readonly string[] QWERTY = new string[]
    {
        "qwertyuiop[]",
        "asdfghjkl;'",
        "zxcvbnm,."
    };

    public static readonly string[] QWERT_CYR = new string[]
    {
        "йцукенгшщзхъ",
        "фывапролджэ",
        "ячсмитьбю"
    };

    public static readonly string[] SKEWMAK = new string[]
    {
        "_,lgwx_zkyb=_",
        "`frstv_hnieu/",
        "jalcd;_.pmboq"
    };


    public static readonly string[] SKEWMAK_CYR = new string[]
    {
        "_щлбшй_чцукэ_",
        "ьпрнтз_гсиеяф",
        "жалвд,_.мыкою"
    };

    public void GetQwertyForSkewmak()
    {
        Dictionary<char, char> EnToCyr=new Dictionary<char, char>();
        Console.OutputEncoding = Encoding.UTF8;
        foreach (var line in SKEWMAK.Zip(SKEWMAK_CYR))
        {
            foreach (var charPair in line.First.Zip(line.Second))
            {
                if (!EnToCyr.ContainsKey(charPair.First) && charPair.First != '_')
                {
                    EnToCyr.Add(charPair.First,charPair.Second);
                }
            }
        }

        var output = QWERTY;
        for (var k = 0; k < output.Length; k++)
        {
            var line = output[k].ToCharArray();
            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (EnToCyr.ContainsKey(c))
                    line[i] = EnToCyr[c];
            }
            Console.WriteLine(new string(line));
        }
    }
}