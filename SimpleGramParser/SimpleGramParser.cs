using GramParser;

namespace SimpleGramParser
{
    public class SimpleGramParser : GramParserBase
    {
        public SimpleGramParser(string strTextToParse)
            : base(strTextToParse)
        {
        }

        public override void Parse(string startSymbol = "")
        {
            _parseTree = GrammarMemo.Instance.Grammar.Parse(TextToParse, startSymbol);
        }
    }
}