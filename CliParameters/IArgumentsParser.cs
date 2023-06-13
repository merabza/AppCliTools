using LibParameters;

namespace CliParameters;

public interface IArgumentsParser
{
    IParameters? Par { get; }
    string? ParametersFileName { get; }
    EParseResult Analysis();
}