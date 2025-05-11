using LibParameters;

namespace DbContextAnalyzer.Models;

public sealed class CreateProjectSeederCodeParameters : IParameters
{
    public string? ProjectPrefix { get; set; }
    public string? ProjectPrefixShort { get; set; }
    public string? LogFolder { get; set; }
    public string? ConnectionStringProd { get; set; }
    public string? ConnectionStringDev { get; set; }
    public string? GetJsonProjectPlacePath { get; set; }
    public string? GetJsonProjectNamespace { get; set; }
    public string? DataSeedingProjectPlacePath { get; set; }
    public string? DataSeedingProjectNamespace { get; set; }
    public string? ExcludesRulesParametersFilePath { get; set; }
    public string? MainDatabaseProjectName { get; set; }
    public string? ProjectDbContextClassName { get; set; }

    public bool CheckBeforeSave()
    {
        return true;
    }
}