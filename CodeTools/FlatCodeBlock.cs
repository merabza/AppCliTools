using System.Text;

namespace CodeTools;

public sealed class FlatCodeBlock : CodeBlockBase, ICodeItem
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public FlatCodeBlock(params object[] codeList) : base(codeList)
    {
    }

    public override string Output(int indentLevel)
    {
        var sb = new StringBuilder();
        foreach (var codeItem in CodeItems) sb.Append(codeItem.Output(indentLevel));

        return sb.ToString();
    }
}