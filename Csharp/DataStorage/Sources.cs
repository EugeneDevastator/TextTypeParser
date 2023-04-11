public static class Sources
{
    public static string WorkingFolder = "d:\\1\\";
    
    public static ParseParams RiderEN = new ParseParams()
    {
        ParsingPath = Path.Combine(WorkingFolder,"toparse_rider"),
        DataPath =  Path.Combine(WorkingFolder,"PARSED_EN"),
        Flags = 0 | WorderFlags.IntelliSense | WorderFlags.SimpleText,
        Languages = new []{ new LanguageEn() }

    };
    public static ParseParams RiderEN_Simple = new ParseParams()
    {
        ParsingPath = Path.Combine(WorkingFolder,"toparse_rider"),
        DataPath = Path.Combine(WorkingFolder,"d:\\1\\PARSED_EN_S"),
        Flags = 0 | WorderFlags.SimpleText,
        Languages = new []{ new LanguageEn() }
    };
    public static ParseParams Cyrilic = new ParseParams()
    {
        ParsingPath = Path.Combine(WorkingFolder,"toparse_cyr"),
        DataPath = Path.Combine(WorkingFolder,"PARSED_CYR"),
        Flags = 0 | WorderFlags.SimpleText,
        Languages = new []{ new LanguageCyr() }
    };
}

public class ParseParams
{
    public string ParsingPath;
    public string DataPath;
    public WorderFlags Flags;
    public Language[] Languages;
}

[Flags]
public enum WorderFlags
{
    None = 0,
    SimpleText = 1,
    IntelliSense = 2,
    First4Letters = 4,
}