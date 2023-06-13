using DbTools;
using LibParameters;

namespace CliParametersDataEdit.Models;

public sealed class DatabaseConnectionParameters : IParameters
{
    public EDataProvider DataProvider { get; set; }
    public string? ConnectionString { get; set; }
    public int CommandTimeOut { get; set; }

    public bool CheckBeforeSave()
    {
        return true;
    }
}