using System;

namespace AppCliTools.CodeTools;

public sealed class CodeExtraLine : ICodeItem
{
    public string Output(int indentLevel)
    {
        string indent = new(' ', indentLevel * Stats.IndentSize);
        return indent + Environment.NewLine;
    }

    public string OutputCreator(int indentLevel, int additionalIndentLevel)
    {
        string indent = new(' ', (indentLevel + additionalIndentLevel) * Stats.IndentSize);
        return "," + Environment.NewLine + indent + "\"\"";
    }
}
