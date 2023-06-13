using System.Text;

namespace LibDataInput.InputParsers;

public sealed class DateParser : InputParser
{
    private readonly DateDelimiterParser _ddp = new();

    public DateParser() : base(' ')
    {
    }

    public override string? TryAddNextChar(string current, char nextChar)
    {
        var res = _ddp.TryAddNextChar(current, nextChar);
        if (res == null)
            return null;

        var sb = new StringBuilder(res);
        TryAddDelimiter(sb);
        return sb.ToString();
    }

    public override bool IsValidNextChar(string current, char nextChar)
    {
        return _ddp.IsValidNextChar(current, nextChar);
    }
}