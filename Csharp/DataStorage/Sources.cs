public static class Sources
{
    public static ParseParams RiderEN = new ParseParams()
    {
        ParsingPath = "d:\\1\\toparse_rider",
        DataPath = "d:\\1\\PARSED_EN",
        Language = LanguageMaps.LettersLowerEn,
        Flags = 0 | WorderFlags.IntelliSense | WorderFlags.SimpleText
    };

    public static ParseParams Cyrilic = new ParseParams()
    {
        ParsingPath = "d:\\1\\toparse_cyr",
        DataPath = "d:\\1\\PARSED_CYR",
        Language = LanguageMaps.LettersLowerRu,
        Flags = 0 | WorderFlags.SimpleText
    };
}

public class ParseParams
{
    public string ParsingPath;
    public string DataPath;
    public string Language;
    public WorderFlags Flags;
}

[Flags]
public enum WorderFlags
{
    None = 0,
    SimpleText = 1,
    IntelliSense = 2,
    First4Letters = 4,
}