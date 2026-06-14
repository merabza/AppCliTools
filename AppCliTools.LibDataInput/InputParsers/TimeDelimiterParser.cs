namespace AppCliTools.LibDataInput.InputParsers;

public sealed class TimeDelimiterParser() : DelimiterParser(':', [0, 0, 0], [23, 59, 59]);
