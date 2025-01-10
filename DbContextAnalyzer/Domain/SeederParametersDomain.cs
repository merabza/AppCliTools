using LibDatabaseParameters;

namespace DbContextAnalyzer.Domain;

public sealed class SeederParametersDomain
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SeederParametersDomain(string jsonFolderName, string secretDataFolder, string logFolder,
        EDatabaseProvider dataProvider, string connectionStringSeed)
    {
        JsonFolderName = jsonFolderName;
        SecretDataFolder = secretDataFolder;
        LogFolder = logFolder;
        ConnectionStringSeed = connectionStringSeed;
        DataProvider = dataProvider;
    }

    public string JsonFolderName { get; set; }
    public string SecretDataFolder { get; set; }
    public string LogFolder { get; set; }
    public EDatabaseProvider DataProvider { get; set; }
    public string ConnectionStringSeed { get; set; }
}