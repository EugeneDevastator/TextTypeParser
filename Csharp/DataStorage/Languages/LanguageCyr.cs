public class LanguageCyr : Language
{
    private string lowers = "цукенгшзхфывапролджэячсмитьбю";
    public override string Visuals => lowers + lowers.ToUpper() + "ёйщъЁЙЩЪ";
    public override string Keys =>    lowers + lowers           + "еишьеишь";
}