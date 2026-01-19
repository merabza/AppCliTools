using System;

namespace AppCliTools.CodeTools;

public sealed class CodeCommand : ICodeItem
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CodeCommand(string commandLine)
    {
        CommandLine = commandLine;
    }

    public string CommandLine { get; set; }

    public string Output(int indentLevel)
    {
        string indent = new(' ', indentLevel * Stats.IndentSize);
        return indent + CommandLine + ";" + Environment.NewLine;
    }

    public string OutputCreator(int indentLevel, int additionalIndentLevel)
    {
        string indent = new(' ', (indentLevel + additionalIndentLevel) * Stats.IndentSize);
        return "," + Environment.NewLine + indent + CommandLine.Quotas();
    }
}
