using System;

namespace CodeTools;

public class OneLineComment : ICodeItem
{
    private readonly string _commentSign;

    public OneLineComment(string commentText, string commentSign = "//")
    {
        _commentSign = commentSign;
        CommentText = commentText;
    }

    public string CommentText { get; set; }

    public string Output(int indentLevel)
    {
        var indent = new string(' ', indentLevel * Stats.IndentSize);
        return indent + _commentSign + CommentText + Environment.NewLine;
    }

    public string OutputCreator(int indentLevel, int additionalIndentLevel)
    {
        var indent = new string(' ', (indentLevel + additionalIndentLevel) * Stats.IndentSize);
        return "," + Environment.NewLine + indent + "new OneLineComment(" + CommentText.Quotas() + ")";
    }
}