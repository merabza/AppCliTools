using System.Collections.Generic;
using System.Linq;

namespace GramParser;

public class ParseTree
{
    public readonly Token? Atom;
    public readonly List<ParseTree> Branches = new();

    public ParseTree()
    {
    }

    public ParseTree(bool isFail)
    {
        IsFail = isFail;
    }

    public ParseTree(Token atom, ParseTree? parent = null)
    {
        Atom = atom;
        Parent = parent;
    }

    public ParseTree(ParseTree parseTree)
    {
        Branches.Add(parseTree);
    }

    public ParseTree? Parent { get; private set; }

    public bool IsFail { get; }

    public string Text { get; set; } = "";

    public void Concatenate(ParseTree parseTree)
    {
        Branches.AddRange(parseTree.Branches);
    }

    public void Append(ParseTree parseTree)
    {
        parseTree.SetParent(this);
        Branches.Add(parseTree);
    }

    private void SetParent(ParseTree parent)
    {
        Parent = parent;
    }

    public void Append(Token atom)
    {
        Branches.Add(new ParseTree(atom, this));
    }


    public bool IsEqual(ParseTree parseTree)
    {
        if (Branches.Count != parseTree.Branches.Count)
            return false;
        if (Branches.Count > 0)
            return !Branches.Where((t, i) => !t.IsEqual(parseTree.Branches[i])).Any();
        return Atom == parseTree.Atom;
    }


    public string ToString(int indent = 0)
    {
        var toRet = new string(' ', indent) + Atom + "\n";
        toRet += new string(' ', indent) + "[\n";
        toRet = Branches.Aggregate(toRet, (current, t) => current + t.ToString(indent + 1));
        toRet += new string(' ', indent) + "]\n";
        return toRet;
    }
}