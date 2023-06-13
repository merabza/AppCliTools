using System;

namespace LibDataInput.InputParsers;

public sealed class DateDelimiterParser : DelimiterParser
{
    public DateDelimiterParser() : base('-', new[] { 1, 1, 1 }, new[] { 9999, 12, 31 })
    {
    }


    protected override int GetMax(int[] digs)
    {
        var i = digs.Length - 1;
        return i < 2 ? Maxes[i] : DateTime.DaysInMonth(digs[0], digs[1]);
    }
}