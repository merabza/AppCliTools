using System.Collections.Generic;
using System.Text;

namespace CodeTools;

public /*open*/ class CodeBlockBase
{
    protected CodeBlockBase()
    {
    }

    protected CodeBlockBase(object?[] codeList)
    {
        foreach (var o in codeList)
        {
            if (o == null)
                continue;
            switch (o)
            {
                case string s:
                    if (s == string.Empty)
                        CodeItems.Add(new CodeExtraLine());
                    else
                        CodeItems.Add(new CodeCommand(s));
                    break;
                case FlatCodeBlock fcb:
                    CodeItems.AddRange(fcb.CodeItems);
                    break;
                case ICodeItem ici:
                    CodeItems.Add(ici);
                    break;
            }
        }
    }

    public List<ICodeItem> CodeItems { get; protected init; } = [];

    public void Add(ICodeItem codeItem)
    {
        CodeItems.Add(codeItem);
    }

    public void AddRange(IEnumerable<ICodeItem> codeItems)
    {
        CodeItems.AddRange(codeItems);
    }

    public virtual string Output(int indentLevel)
    {
        var sb = new StringBuilder();
        foreach (var codeItem in CodeItems)
            sb.Append(codeItem.Output(indentLevel + 1));
        return sb.ToString();
    }

    public virtual string OutputCreator(int indentLevel, int additionalIndentLevel)
    {
        var sb = new StringBuilder();
        foreach (var codeItem in CodeItems) sb.Append(codeItem.OutputCreator(indentLevel + 1, additionalIndentLevel));

        return sb.ToString();
    }
}