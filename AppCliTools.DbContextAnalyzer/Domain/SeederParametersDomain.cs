using ParametersManagement.LibDatabaseParameters;

namespace AppCliTools.DbContextAnalyzer.Domain;

public sealed class SeederParametersDomain
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SeederParametersDomain(string jsonFolderName, string secretDataFolder, string logFolder,
        EDatabaseProvider dataProvider, string connectionStringSeed, int commandTimeoutSeed,
        string excludesRulesParametersFilePath)
    {
        JsonFolderName = jsonFolderName;
        SecretDataFolder = secretDataFolder;
        LogFolder = logFolder;
        ConnectionStringSeed = connectionStringSeed;
        CommandTimeoutSeed = commandTimeoutSeed;
        ExcludesRulesParametersFilePath = excludesRulesParametersFilePath;
        DataProvider = dataProvider;
    }

    public string JsonFolderName { get; set; }
    public string SecretDataFolder { get; set; }
    public string LogFolder { get; set; }
    public EDatabaseProvider DataProvider { get; set; }
    public string ConnectionStringSeed { get; set; }
    public int CommandTimeoutSeed { get; }
    public string ExcludesRulesParametersFilePath { get; }
}
