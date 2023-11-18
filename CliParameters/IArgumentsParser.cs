using LibParameters;
using System.Collections.Generic;

namespace CliParameters;

public interface IArgumentsParser
{
    IParameters? Par { get; }
    string? ParametersFileName { get; }
    public List<string> Switches { get; }
    EParseResult Analysis();
}