namespace AppCliTools.CliTools;

public interface IRecentCommandParameters
{
    string? RecentCommandsFileName { get; set; }
    int RecentCommandsCount { get; set; }
}
