using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeTools;

public sealed class CodeBlock : CodeBlockBase, ICodeItem
{
    public CodeBlock(string blockHeader, params object?[] codeList) : base(codeList)
    {
        BlockHeader = blockHeader;
        BlockInOneLine = false;
        Initializer = null;
    }

    public CodeBlock(string blockHeader, bool blockInOneLine, params object[] codeList) : base(codeList)
    {
        BlockHeader = blockHeader;
        BlockInOneLine = blockInOneLine;
        Initializer = null;
    }

    public CodeBlock(string blockHeader, string initializer, bool blockInOneLine, params object[] codeList) :
        base(codeList)
    {
        BlockHeader = blockHeader;
        BlockInOneLine = blockInOneLine;
        Initializer = initializer;
    }

    public string BlockHeader { get; set; }
    public string? Initializer { get; set; }
    public bool BlockInOneLine { get; set; }

    public override string Output(int indentLevel)
    {
        var indent = new string(' ', indentLevel * Stats.IndentSize);
        var sb = new StringBuilder();
        if (BlockInOneLine && CodeItems.All(a => a is CodeCommand))
        {
            var commandsList = new List<string> { BlockHeader, "{" };
            commandsList.AddRange(CodeItems.Cast<CodeCommand>().Select(s => s.CommandLine + ";"));
            commandsList.Add("}");
            if (Initializer is not null)
            {
                commandsList.Add(Initializer);
                commandsList.Add(";");
            }

            sb.AppendLine(indent + string.Join(" ", commandsList));
        }
        else
        {
            sb.AppendLine(indent + BlockHeader);
            sb.AppendLine(indent + "{");
            sb.Append(base.Output(indentLevel));
            sb.AppendLine(indent + "}");
        }

        return sb.ToString();
    }

    public override string OutputCreator(int indentLevel, int additionalIndentLevel)
    {
        var indent = indentLevel == 0
            ? string.Empty
            : new string(' ', (indentLevel + additionalIndentLevel) * Stats.IndentSize);
        var sb = new StringBuilder();
        if (indentLevel > 0) sb.Append("," + Environment.NewLine + indent);
        if (BlockInOneLine && CodeItems.All(a => a is CodeCommand))
        {
            sb.Append($"new CodeBlock({BlockHeader.Quotas()}");
            if (Initializer is not null)
                sb.Append($", {Initializer.Quotas()}");
            sb.Append(", true");
            sb.Append(CodeItems.Cast<CodeCommand>().Select(s => s.CommandLine.Quotas()));
            sb.Append(")");
        }
        else
        {
            sb.Append($"new CodeBlock({BlockHeader.Quotas()}");
            sb.Append(base.OutputCreator(indentLevel, additionalIndentLevel));
            sb.Append(")");
        }

        return sb.ToString();
    }
}