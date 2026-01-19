using ParametersManagement.LibParameters;

namespace AppCliTools.DbContextAnalyzer.Models;

public sealed class CreatorCreatorParameters : IParameters
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CreatorCreatorParameters(string projectPrefix, string dbProjectNamespace, string projectDbContextClassName,
        string projectPrefixShort, string createSeederCodeProjectNamespace, string createSeederCodeProjectPlacePath,
        string getJsonProjectNamespace, string getJsonProjectPlacePath, string seedProjectNamespace,
        string seedProjectPlacePath, string seedConnectionStringParameterName, string seedParametersFileName,
        string dbContextProjectName, string dbContextClassName)
    {
        ProjectPrefix = projectPrefix;
        DbProjectNamespace = dbProjectNamespace;
        ProjectDbContextClassName = projectDbContextClassName;
        ProjectPrefixShort = projectPrefixShort;
        CreateSeederCodeProjectNamespace = createSeederCodeProjectNamespace;
        CreateSeederCodeProjectPlacePath = createSeederCodeProjectPlacePath;
        GetJsonProjectNamespace = getJsonProjectNamespace;
        GetJsonProjectPlacePath = getJsonProjectPlacePath;
        SeedProjectNamespace = seedProjectNamespace;
        SeedProjectPlacePath = seedProjectPlacePath;
        SeedConnectionStringParameterName = seedConnectionStringParameterName;
        SeedParametersFileName = seedParametersFileName;
        DbContextProjectName = dbContextProjectName;
        DbContextClassName = dbContextClassName;
    }

    public string DbProjectNamespace { get; }
    public string ProjectDbContextClassName { get; }
    public string ProjectPrefix { get; }
    public string ProjectPrefixShort { get; }
    public string CreateSeederCodeProjectNamespace { get; }
    public string CreateSeederCodeProjectPlacePath { get; }
    public string GetJsonProjectNamespace { get; }
    public string GetJsonProjectPlacePath { get; }
    public string SeedProjectNamespace { get; }
    public string SeedProjectPlacePath { get; }
    public string SeedConnectionStringParameterName { get; }
    public string SeedParametersFileName { get; }
    public string DbContextProjectName { get; }
    public string DbContextClassName { get; }

    public bool CheckBeforeSave()
    {
        return true;
    }
}
