using DbTools;

namespace DbContextAnalyzer.Domain;

public sealed class SeederParametersDomain
{
    public SeederParametersDomain(string jsonFolderName, string secretDataFolder, string logFolder,
        EDataProvider dataProvider, string connectionStringSeed)
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
    public EDataProvider DataProvider { get; set; }
    public string ConnectionStringSeed { get; set; }
}