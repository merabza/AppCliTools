using System;

namespace CodeTools;

public sealed class TextCode : ICodeItem
{
    public TextCode(string text)
    {
        Text = text;
    }

    public string Text { get; set; }

    public string Output(int indentLevel)
    {
        return Text + Environment.NewLine;
    }

    public string OutputCreator(int indentLevel, int additionalIndentLevel)
    {
        var indent = new string(' ', (indentLevel + additionalIndentLevel) * Stats.IndentSize);
        return "," + Environment.NewLine + indent + Text.Quotas();
    }
}