namespace GramParser;

public class Token
{
    protected readonly object? PVal;


    protected Token()
    {
        Tok = null;
        PVal = null;
    }

    protected Token(string tok)
    {
        Tok = tok;
        PVal = null;
    }

    public string? Tok { get; }

    public object? Val => PVal;
    public virtual string TokenName => "";
}