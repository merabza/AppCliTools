using System.Collections.Generic;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters;

public interface IArgumentsParser<out T> where T : class, IParameters, new()
{
    T? Par { get; }
    string? ParametersFileName { get; }
    List<string> Switches { get; }
    EParseResult Analysis();
}
