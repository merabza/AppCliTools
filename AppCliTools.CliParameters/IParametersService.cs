using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters;

public interface IParametersService<out T> where T : class, IParameters, new()
{
    T? Par { get; }
    string? ParametersFileName { get; }
    EParseResult Analysis(string? parametersFileName);
}
