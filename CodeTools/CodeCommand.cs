using System;

namespace CodeTools;

public sealed class CodeCommand : ICodeItem
{
    public CodeCommand(string commandLine)
    {
        CommandLine = commandLine;
    }

    public string CommandLine { get; set; }

    public string Output(int indentLevel)
    {
        var indent = new string(' ', indentLevel * Stats.IndentSize);
        return indent + CommandLine + ";" + Environment.NewLine;
    }

    public string OutputCreator(int indentLevel, int additionalIndentLevel)
    {
        var indent = new string(' ', (indentLevel + additionalIndentLevel) * Stats.IndentSize);
        return "," + Environment.NewLine + indent + CommandLine.Quotas();
    }
}