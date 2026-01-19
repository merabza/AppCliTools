namespace AppCliTools.GramParser;

public class GrammarToken : Token
{
    private readonly string _pName;

    public GrammarToken(string name)
    {
        _pName = name;
    }

    public override string TokenName => _pName;

    public override string ToString()
    {
        return _pName;
    }
}
