using System.Text;

namespace CodeTools;

public sealed class CodeEnum : ICodeItem
{
    private readonly string[] _enumElementsList;
    private readonly string _enumHeader;


    public CodeEnum(string enumHeader, params string[] enumElementsList)
    {
        _enumHeader = enumHeader;
        _enumElementsList = enumElementsList;
    }

    public string Output(int indentLevel)
    {
        var indent = new string(' ', indentLevel * Stats.IndentSize);
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine(indent + _enumHeader);
        sb.AppendLine(indent + "{");
        var elementIndent = new string(' ', (indentLevel + 1) * Stats.IndentSize);
        foreach (var enumElement in _enumElementsList) sb.AppendLine(elementIndent + enumElement);
        sb.AppendLine(indent + "}");

        return sb.ToString();
    }

    public string OutputCreator(int indentLevel, int additionalIndentLevel)
    {
        var indent = new string(' ', indentLevel * Stats.IndentSize);
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine(indent + _enumHeader);
        sb.AppendLine(indent + "{");
        var elementIndent = new string(' ', (indentLevel + 1) * Stats.IndentSize);
        foreach (var enumElement in _enumElementsList) sb.AppendLine(elementIndent + enumElement);
        sb.AppendLine(indent + "}");

        return sb.ToString();
    }
}

/*
  public sealed class CodeRegion : CodeBlockBase, ICodeItem
  {
    public string RegionHeader { get; set; }

    public CodeRegion(string regionHeader, params object[] codeList) : base(codeList)
    {
      RegionHeader = regionHeader;
    }

    public override string Output(int indentLevel)
    {
      string indent = new string(' ', indentLevel * Stats.IndentSize);
      StringBuilder sb = new StringBuilder();

      sb.AppendLine();
      sb.AppendLine(indent + $"#region {RegionHeader}");
      sb.AppendLine(indent + "");
      sb.Append(base.Output(indentLevel));
      sb.AppendLine(indent + "");
      sb.AppendLine(indent + "#endregion");

      return sb.ToString();
    }
  }
 */