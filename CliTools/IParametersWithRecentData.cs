using LibParameters;

namespace CliTools;

public interface IParametersWithRecentData : IParameters
{
    string RecentCommandsFileName { get; }
    int RecentCommandsCount { get; }
}