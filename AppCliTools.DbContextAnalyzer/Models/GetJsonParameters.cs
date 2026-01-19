using ParametersManagement.LibParameters;

namespace AppCliTools.DbContextAnalyzer.Models;

public sealed class GetJsonParameters : IParameters
{
    public string? JsonFolderName { get; set; }
    public string? LogFolder { get; set; }
    public string? ConnectionStringProd { get; set; }

    public bool CheckBeforeSave()
    {
        return true;
    }
}
