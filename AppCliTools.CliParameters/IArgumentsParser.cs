using System.Collections.Generic;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters;

public interface IArgumentsParser
{
    IParameters? Par { get; }
    string? ParametersFileName { get; }
    List<string> Switches { get; }
    EParseResult Analysis();
}
