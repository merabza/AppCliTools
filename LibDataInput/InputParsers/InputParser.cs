using System;
using System.Text;

namespace LibDataInput.InputParsers;

public /*open*/ class InputParser
{
    protected readonly char Delimiter;

    protected InputParser(char delimiter)
    {
        Delimiter = delimiter;
    }

    public virtual string? TryAddNextChar(string current, char nextChar)
    {
        var sb = new StringBuilder(current);
        if (!IsValidNextChar(current, nextChar))
        {
            return null;
        }

        sb.Append(nextChar);
        Console.Write(nextChar);
        TryAddDelimiter(sb);
        return sb.ToString();
    }

    protected void TryAddDelimiter(StringBuilder sb)
    {
        if (!CanAddDelimiter(sb.ToString()))
        {
            return;
        }

        sb.Append(Delimiter);
        Console.Write(Delimiter);
    }

    public virtual bool IsValidNextChar(string current, char nextChar)
    {
        return false;
    }

    public virtual bool CanAddDelimiter(string current)
    {
        return false;
    }
}
