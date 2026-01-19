using System;

namespace AppCliTools.LibDataInput.InputParsers;

public sealed class DateDelimiterParser : DelimiterParser
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DateDelimiterParser() : base('-', [1, 1, 1], [9999, 12, 31])
    {
    }

    protected override int GetMax(int[] digs)
    {
        int i = digs.Length - 1;
        return i < 2 ? Maxes[i] : DateTime.DaysInMonth(digs[0], digs[1]);
    }
}
