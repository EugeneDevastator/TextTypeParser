using System.Text;

/// <summary>
/// Letter - raw semantic for letters of alphabet
/// Symbol - special character (non letter)
/// Sign - letter or symbol or space etc.
/// Key - meaning keyboard key in its unshifted variation
/// Visual - sign that we see in text (shifted + unshifted)
/// </summary>

public class Language
{
    public virtual string Visuals { get; set;  }
    public virtual string Keys { get; set;  }

    public void Write(string fname)
    {
        var text = new []{ Visuals , Keys };
        File.WriteAllLines(fname,text,Encoding.UTF8);
    }

    public static Language ReadFromFile(string fname)
    {
        var lines  = File.ReadAllLines(fname, Encoding.UTF8);
        return new Language()
        {
            Visuals = lines[0],
            Keys = lines[1]
        };
    }
}