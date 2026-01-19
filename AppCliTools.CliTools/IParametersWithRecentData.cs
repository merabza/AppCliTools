using ParametersManagement.LibParameters;

namespace AppCliTools.CliTools;

public interface IParametersWithRecentData : IParameters
{
    string? RecentCommandsFileName { get; }
    int RecentCommandsCount { get; }
}
