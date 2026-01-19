using System.Text;

namespace AppCliTools.CodeTools;

public sealed class CodeEnum : ICodeItem
{
    private readonly string[] _enumElementsList;
    private readonly string _enumHeader;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CodeEnum(string enumHeader, params string[] enumElementsList)
    {
        _enumHeader = enumHeader;
        _enumElementsList = enumElementsList;
    }

    public string Output(int indentLevel)
    {
        string indent = new(' ', indentLevel * Stats.IndentSize);
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine(indent + _enumHeader);
        sb.AppendLine(indent + "{");
        string elementIndent = new(' ', (indentLevel + 1) * Stats.IndentSize);
        foreach (string enumElement in _enumElementsList)
        {
            sb.AppendLine(elementIndent + enumElement);
        }

        sb.AppendLine(indent + "}");

        return sb.ToString();
    }

    public string OutputCreator(int indentLevel, int additionalIndentLevel)
    {
        string indent = new(' ', indentLevel * Stats.IndentSize);
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine(indent + _enumHeader);
        sb.AppendLine(indent + "{");
        string elementIndent = new(' ', (indentLevel + 1) * Stats.IndentSize);
        foreach (string enumElement in _enumElementsList)
        {
            sb.AppendLine(elementIndent + enumElement);
        }

        sb.AppendLine(indent + "}");

        return sb.ToString();
    }
}
