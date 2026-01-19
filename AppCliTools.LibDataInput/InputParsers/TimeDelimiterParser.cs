namespace AppCliTools.LibDataInput.InputParsers;

public sealed class TimeDelimiterParser : DelimiterParser
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public TimeDelimiterParser() : base(':', [0, 0, 0], [23, 59, 59])
    {
    }
}
