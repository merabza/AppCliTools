namespace DbContextAnalyzer.Models;

public sealed class SeederCodeCreatorParameters
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SeederCodeCreatorParameters(string projectPrefix, string projectPrefixShort, string modelsFolderName,
        string projectSeedersFolderName, string carcassSeedersFolderName, string placePath, string projectNamespace,
        string dataSeederRepositoryInterfaceName, string keyNameIdRepositoryInterfaceName,
        string projectDataSeedersFactoryClassName, string dbProjectNamespace, string projectDbContextClassName,
        string dbProjectModelsFolderName, string dataSeederBaseClassName)
    {
        ProjectPrefix = projectPrefix;
        ProjectPrefixShort = projectPrefixShort;
        ModelsFolderName = modelsFolderName;
        ProjectSeedersFolderName = projectSeedersFolderName;
        CarcassSeedersFolderName = carcassSeedersFolderName;
        PlacePath = placePath;
        ProjectNamespace = projectNamespace;
        DataSeederRepositoryInterfaceName = dataSeederRepositoryInterfaceName;
        KeyNameIdRepositoryInterfaceName = keyNameIdRepositoryInterfaceName;
        ProjectDataSeedersFactoryClassName = projectDataSeedersFactoryClassName;
        DbProjectNamespace = dbProjectNamespace;
        ProjectDbContextClassName = projectDbContextClassName;
        DbProjectModelsFolderName = dbProjectModelsFolderName;
        DataSeederBaseClassName = dataSeederBaseClassName;
    }

    public string ProjectPrefix { get; set; }
    public string ProjectPrefixShort { get; set; }
    public string PlacePath { get; set; }
    public string ProjectNamespace { get; set; }
    public string ModelsFolderName { get; set; }
    public string ProjectSeedersFolderName { get; set; }
    public string CarcassSeedersFolderName { get; set; }
    public string DataSeederRepositoryInterfaceName { get; set; }
    public string KeyNameIdRepositoryInterfaceName { get; set; }
    public string ProjectDataSeedersFactoryClassName { get; set; }
    public string DbProjectNamespace { get; set; }
    public string ProjectDbContextClassName { get; set; }
    public string DbProjectModelsFolderName { get; set; }

    public string DataSeederBaseClassName { get; set; }
}