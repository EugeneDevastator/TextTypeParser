public class LanguageSym : Language
{
    public override string Visuals => " `~9(0):;" + '\"' + "\'" + ",<.>/?-_=+[{]}" + '\\' + "|";
    public override string Keys =>    " ``9900;;" + "\'" + "\'" + ",,..//--==[[]]" + '\\' + '\\';
}