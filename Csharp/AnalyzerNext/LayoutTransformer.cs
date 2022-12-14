using System.Text;

namespace AnalyzerNext;

public class LayoutTransformer
{
    public static readonly string[] QWERTY = new string[]
    {
        "qwertyuiop[]\\",
        "asdfghjkl;'",
        "zxcvbnm,./"
    };

    public static readonly string[] QWERT_CYR = new string[]
    {
        "йцукенгшщзхъ",
        "фывапролджэ",
        "ячсмитьбю"
    };

    public static readonly string[] SKEWMAK = new string[]
    {
        "_,lgwf_xkyb\\=",
        "`urstv_hniez/",
        "jalcd;_.pmboq"
    };


    public static readonly string[] SKEWMAK_CYR = new string[]
    {
        "_цлбпш_хжукэ_",
        "ьпрнтз_гсиеяф",
        "чалвд,_.мыкою"
    };

    public void GetQwertyForSkewmak()
    {
        Dictionary<char, char> EnToCyr = new Dictionary<char, char>();
        Console.OutputEncoding = Encoding.UTF8;
        foreach (var line in SKEWMAK.Zip(SKEWMAK_CYR))
        {
            foreach (var charPair in line.First.Zip(line.Second))
            {
                if (!EnToCyr.ContainsKey(charPair.First) && charPair.First != '_')
                {
                    EnToCyr.Add(charPair.First, charPair.Second);
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

    public Dictionary<char, string> ENtoPlacement = new Dictionary<char, string>()
    {
        { '1', $"02" },
        { '2', $"03" },
        { '3', $"04" },
        { '4', $"05" },
        { '5', $"06" },
        { '6', $"07" },
        { '7', $"08" },
        { '8', $"09" },
        { '9', $"0a" },
        { '0', $"0b" },
        { '-', $"0c" },
        { '+', $"0d" },
        { 'Q', $"10" },
        { 'W', $"11" },
        { 'E', $"12" },
        { 'R', $"13" },
        { 'T', $"14" },
        { 'Y', $"15" },
        { 'U', $"16" },
        { 'I', $"17" },
        { 'O', $"18" },
        { 'P', $"19" },
        { '[', $"1a" },
        { ']', $"1b" },
        { 'A', $"1e" },
        { 'S', $"1f" },
        { 'D', $"20" },
        { 'F', $"21" },
        { 'G', $"22" },
        { 'H', $"23" },
        { 'J', $"24" },
        { 'K', $"25" },
        { 'L', $"26" },
        { ',', $"27" },
        { '\'', $"28" },
        { '`', $"29" },
        { '\\', $"2b" },
        { 'Z', $"2c" },
        { 'X', $"2d" },
        { 'C', $"2e" },
        { 'V', $"2f" },
        { 'B', $"30" },
        { 'N', $"31" },
        { 'M', $"32" },
        { ',', $"33" },
        { ';', $"34" },
        { '/', $"35" },
        { ' ', $"39" },
        { '.', $"53" },
//{'\\ 56		0	005c	002f	001c	-1		// REVERSE SOLIDUS, SOLIDUS, INFORMATION SEPARATOR FOUR, <none> most likely unused left oem key.
    };

    public Dictionary<char, string> CharToHexCode = new Dictionary<char, string>()
    {
        // https://en.wikipedia.org/wiki/Cyrillic_script_in_Unicode#:~:text=Cyrillic%3A%20U%2B0400%E2%80%93U,%E2%80%93U%2B052F%2C%2048%20characters
        { 'А', "0410" },
        { 'Б', "0411" },
        { 'В', "0412" },
        { 'Г', "0413" },
        { 'Д', "0414" },
        { 'Е', "0415" },
        { 'Ж', "0416" },
        { 'З', "0417" },
        { 'И', "0418" },
        { 'Й', "0419" },
        { 'К', "041A" },
        { 'Л', "041B" },
        { 'М', "041C" },
        { 'Н', "041D" },
        { 'О', "041E" },
        { 'П', "041F" },
        { 'Р', "0420" },
        { 'С', "0421" },
        { 'Т', "0422" },
        { 'У', "0423" },
        { 'Ф', "0424" },
        { 'Х', "0425" },
        { 'Ц', "0426" },
        { 'Ч', "0427" },
        { 'Ш', "0428" },
        { 'Щ', "0429" },
        { 'Ъ', "042A" },
        { 'Ы', "042B" },
        { 'Ь', "042C" },
        { 'Э', "042D" },
        { 'Ю', "042E" },
        { 'Я', "042F" },
        { 'а', "0430" },
        { 'б', "0431" },
        { 'в', "0432" },
        { 'г', "0433" },
        { 'д', "0434" },
        { 'е', "0435" },
        { 'ж', "0436" },
        { 'з', "0437" },
        { 'и', "0438" },
        { 'й', "0439" },
        { 'к', "043A" },
        { 'л', "043B" },
        { 'м', "043C" },
        { 'н', "043D" },
        { 'о', "043E" },
        { 'п', "043F" },
        { 'р', "0440" },
        { 'с', "0441" },
        { 'т', "0442" },
        { 'у', "0443" },
        { 'ф', "0444" },
        { 'х', "0445" },
        { 'ц', "0446" },
        { 'ч', "0447" },
        { 'ш', "0448" },
        { 'щ', "0449" },
        { 'ъ', "044A" },
        { 'ы', "044B" },
        { 'ь', "044C" },
        { 'э', "044D" },
        { 'ю', "044E" },
        { 'я', "044F" },
    };
}