using System;

namespace CodeTools;

public sealed class OneLineComment : ICodeItem
{
    public OneLineComment(string commentText)
    {
        CommentText = commentText;
    }

    public string CommentText { get; set; }

    public string Output(int indentLevel)
    {
        var indent = new string(' ', indentLevel * Stats.IndentSize);
        return indent + "//" + CommentText + Environment.NewLine;
    }

    public string OutputCreator(int indentLevel, int additionalIndentLevel)
    {
        var indent = new string(' ', (indentLevel + additionalIndentLevel) * Stats.IndentSize);
        return "," + Environment.NewLine + indent + "new OneLineComment(" + CommentText.Quotas() + ")";
    }
}