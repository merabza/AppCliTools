namespace LibDataInput.InputParsers;

public sealed class TimeDelimiterParser : DelimiterParser
{
    public TimeDelimiterParser() : base(':', new[] { 0, 0, 0 }, new[] { 23, 59, 59 })
    {
    }
}