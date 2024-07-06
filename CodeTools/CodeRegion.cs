using System.Text;

namespace CodeTools;

public sealed class CodeRegion : CodeBlockBase, ICodeItem
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CodeRegion(string regionHeader, params object[] codeList) : base(codeList)
    {
        RegionHeader = regionHeader;
    }

    public string RegionHeader { get; set; }

    public override string Output(int indentLevel)
    {
        var indent = new string(' ', indentLevel * Stats.IndentSize);
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine(indent + $"#region {RegionHeader}");
        sb.AppendLine(indent + string.Empty);
        sb.Append(base.Output(indentLevel));
        sb.AppendLine(indent + string.Empty);
        sb.AppendLine(indent + "#endregion");

        return sb.ToString();
    }

    public override string OutputCreator(int indentLevel, int additionalIndentLevel)
    {
        var indent = new string(' ', (indentLevel + additionalIndentLevel) * Stats.IndentSize);
        var sb = new StringBuilder();
        sb.Append(indent + $"new CodeRegion({RegionHeader.Quotas()}");
        sb.Append(base.Output(indentLevel));
        sb.AppendLine(")");

        return sb.ToString();
    }
}