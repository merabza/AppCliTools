using System;

namespace AppCliTools.CodeTools;

public sealed class TextCode : ICodeItem
{
    // ReSharper disable once ConvertToPrimaryConstructor
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
        string indent = new(' ', (indentLevel + additionalIndentLevel) * Stats.IndentSize);
        return "," + Environment.NewLine + indent + Text.Quotas();
    }
}
