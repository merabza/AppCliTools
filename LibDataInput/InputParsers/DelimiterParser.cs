using System;
using System.Collections.Generic;
using System.Linq;

namespace LibDataInput.InputParsers;

public /*open*/ class DelimiterParser : InputParser
{
    private readonly int _count;
    private readonly int[] _digits;

    private readonly int[] _minimums;
    protected readonly int[] Maxes;

    protected DelimiterParser(char delimiter, int[] minimums, int[] maxes) : base(delimiter)
    {
        _minimums = minimums;
        Maxes = maxes;
        _digits = maxes.Select(s => (int)-Math.Floor(-Math.Log10(s))).ToArray();
        _count = maxes.Length;
    }

    protected virtual int GetMax(int[] digs)
    {
        return Maxes[digs.Length - 1];
    }


    public override bool CanAddDelimiter(string current)
    {
        var digs = current.Split(Delimiter);
        if (digs.Length > _count - 1)
            return false;

        var i = digs.Length - 1;
        var lastOne = digs[i];

        if (lastOne.Length == _digits[i])
            return true;

        if (!int.TryParse(lastOne, out var number))
            return false;

        return number * 10 > GetMax(digs.Select(int.Parse).ToArray());
    }


    public override bool IsValidNextChar(string current, char nextChar)
    {
        if (!char.IsDigit(nextChar) && nextChar != Delimiter)
            return false;


        var checkString = $"{current}{nextChar}";

        var digs = checkString.Split(Delimiter);

        if (digs.Length > _count)
            return false;

        var parsedNumbers = new List<int>();
        for (var i = 0; i < digs.Length; i++)
        {
            var dig = digs[i];
            var len = dig.Length;
            if (len > _digits[i])
                return false;
            if (i < digs.Length - 1 && len < 1)
                return false;
            if (dig == string.Empty)
                continue;
            if (!int.TryParse(dig, out var number))
                return false;
            parsedNumbers.Add(number);
            if (dig.Length == _digits[i] && number < _minimums[i])
                return false;
            if (number > GetMax(parsedNumbers.ToArray()))
                return false;
        }

        return true;
    }
}