using LibParameters;

namespace DbContextAnalyzer.Domain;

public sealed class CreateProjectSeederCodeParametersDomain : IParameters
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CreateProjectSeederCodeParametersDomain(string projectPrefix, string projectPrefixShort, string logFolder,
        string connectionStringProd, string getJsonProjectPlacePath, string getJsonProjectNamespace,
        string dataSeedingProjectPlacePath, string dataSeedingProjectNamespace, string excludesRulesParametersFilePath,
        string mainDatabaseProjectName, string projectDbContextClassName)
    {
        ProjectPrefix = projectPrefix;
        ProjectPrefixShort = projectPrefixShort;
        LogFolder = logFolder;
        ConnectionStringProd = connectionStringProd;
        GetJsonProjectPlacePath = getJsonProjectPlacePath;
        GetJsonProjectNamespace = getJsonProjectNamespace;
        DataSeedingProjectPlacePath = dataSeedingProjectPlacePath;
        DataSeedingProjectNamespace = dataSeedingProjectNamespace;
        ExcludesRulesParametersFilePath = excludesRulesParametersFilePath;
        MainDatabaseProjectName = mainDatabaseProjectName;
        ProjectDbContextClassName = projectDbContextClassName;
    }

    public string ProjectPrefix { get; }

    public string ProjectPrefixShort { get; }

    public string LogFolder { get; }
    public string ConnectionStringProd { get; }
    public string GetJsonProjectPlacePath { get; }
    public string GetJsonProjectNamespace { get; }
    public string DataSeedingProjectPlacePath { get; }
    public string DataSeedingProjectNamespace { get; }
    public string ExcludesRulesParametersFilePath { get; }
    public string MainDatabaseProjectName { get; }
    public string ProjectDbContextClassName { get; }

    public bool CheckBeforeSave()
    {
        return true;
    }
}