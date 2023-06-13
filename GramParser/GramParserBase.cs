namespace GramParser;

public class GramParserBase
{
    protected readonly string TextToParse;
    protected ParseTree? _parseTree;

    protected GramParserBase(string strTextToParse)
    {
        TextToParse = strTextToParse;
    }

    public ParseTree? ParseTree => _parseTree;

    public virtual void Parse(string startSymbol)
    {
    }
}