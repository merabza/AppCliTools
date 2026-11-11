using System;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit.CliMenuCommands;

//სერვერთან კავშირის პარამეტრებიდან ConnectionString-ის დაგენერირება და ეკრანზე გამოტანა
public sealed class GenerateConnectionStringCliMenuCommand : CliMenuCommand
{
    private readonly string _dbServerName;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GenerateConnectionStringCliMenuCommand(string dbServerName, IParametersManager parametersManager) : base(
        "Generate Connection String", EMenuAction.Reload)
    {
        _dbServerName = dbServerName;
        _parametersManager = parametersManager;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var parameters = (IParametersWithDatabaseServerConnections)_parametersManager.Parameters;
        if (!parameters.DatabaseServerConnections.TryGetValue(_dbServerName,
                out DatabaseServerConnectionData? databaseServerConnection))
        {
            StShared.WriteErrorLine($"Database Server Connection {_dbServerName} not found", true);
            return ValueTask.FromResult(false);
        }

        string? connectionString =
            DatabaseServerConnectionStringConverter.CreateConnectionString(databaseServerConnection);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            StShared.WriteErrorLine(
                $"Connection String can not be created for {databaseServerConnection.DatabaseServerProvider} provider",
                true);
            return ValueTask.FromResult(false);
        }

        Console.WriteLine("Connection String:");
        Console.WriteLine(connectionString);
        //მენიუ ეკრანს ასუფთავებს, ამიტომ შედეგის წასაკითხად პაუზაა საჭირო
        StShared.Pause();
        return ValueTask.FromResult(true);
    }
}
