﻿using System;

namespace CodeTools;

public sealed class OneLineComment : ICodeItem
{
    private readonly string _commentSign;

    // ReSharper disable once ConvertToPrimaryConstructor
    public OneLineComment(string commentText, string commentSign = "//")
    {
        _commentSign = commentSign;
        CommentText = commentText;
    }

    private string CommentText { get; }

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