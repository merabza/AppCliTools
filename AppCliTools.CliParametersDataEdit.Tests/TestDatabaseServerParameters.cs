using System.Collections.Generic;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;

namespace AppCliTools.CliParametersDataEdit.Tests;

//the parameters a DatabaseServerConnectionCruder needs: its connections and the api clients of CheckValidation
internal sealed class TestDatabaseServerParameters : IParametersWithDatabaseServerConnections,
    IParametersWithApiClients
{
    public Dictionary<string, ApiClientSettings> ApiClients { get; } = [];
    public Dictionary<string, DatabaseServerConnectionData> DatabaseServerConnections { get; } = [];

    public bool CheckBeforeSave()
    {
        return true;
    }
}
