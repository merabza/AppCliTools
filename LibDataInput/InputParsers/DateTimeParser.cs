using System;
using System.Globalization;
using System.Text;

namespace LibDataInput.InputParsers;

public sealed class DateTimeParser : InputParser
{
    private readonly DateDelimiterParser _ddp = new();
    private readonly TimeDelimiterParser _tdp = new();

    // ReSharper disable once ConvertToPrimaryConstructor
    public DateTimeParser() : base(' ')
    {
    }

    public override string? TryAddNextChar(string current, char nextChar)
    {
        if (current.Contains(Delimiter))
        {
            if (nextChar == Delimiter)
            {
                return null;
            }

            string[] spl = current.Split(Delimiter);
            if (spl.Length != 2)
            {
                return null;
            }

            string? nc = _tdp.TryAddNextChar(spl[1], nextChar);
            if (nc == null)
            {
                return null;
            }

            spl[1] = nc;
            return string.Join(Delimiter, spl);
        }

        if (nextChar == Delimiter)
        {
            if (_ddp.IsValidNextChar(current, '0') || _ddp.CanAddDelimiter(current))
            {
                return null;
            }

            //if (!CanAddDelimiter(current))
            //  return null;
            Console.Write(nextChar);
            return $"{current}{nextChar}";
        }

        string? res = _ddp.TryAddNextChar(current, nextChar);
        if (res == null)
        {
            return null;
        }

        var sb = new StringBuilder(res);
        TryAddDelimiter(sb);
        return sb.ToString();
    }

    public override bool IsValidNextChar(string current, char nextChar)
    {
        if (current.Contains(Delimiter))
        {
            if (nextChar == Delimiter)
            {
                return false;
            }

            string[] spl = current.Split(Delimiter);
            if (spl.Length != 2)
            {
                return false;
            }

            if (!_tdp.IsValidNextChar(spl[1], nextChar))
            {
                return false;
            }
        }
        else if (nextChar == Delimiter)
        {
            //if (ddp.IsValidNextChar(current, '0') || ddp.CanAddDelimiter(current))
            //  return false;
            if (!CanAddDelimiter(current))
            {
                return false;
            }
        }
        else
        {
            if (!_ddp.IsValidNextChar(current, nextChar))
            {
                return false;
            }
        }

        return true;
    }

    public override bool CanAddDelimiter(string current)
    {
        if (current.Contains(Delimiter))
        {
            return false;
        }

        if (!DateTime.TryParse(current, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return false;
        }

        return !_ddp.IsValidNextChar(current, '0') && !_ddp.CanAddDelimiter(current);
    }
}
