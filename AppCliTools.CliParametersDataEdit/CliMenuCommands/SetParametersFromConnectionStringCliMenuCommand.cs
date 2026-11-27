using System;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit.CliMenuCommands;

//შეყვანილი ConnectionString-იდან სერვერთან კავშირის პარამეტრების დაყენება
public sealed class SetParametersFromConnectionStringCliMenuCommand : CliMenuCommand
{
    private readonly string _dbServerName;
    private readonly Func<string?> _inputConnectionString;
    private readonly IParametersManager _parametersManager;

    public SetParametersFromConnectionStringCliMenuCommand(string dbServerName, IParametersManager parametersManager) :
        this(dbServerName, parametersManager, InputConnectionString)
    {
    }

    //ტესტებისთვის: Inputer კონსოლს პირდაპირ კითხულობს
    internal SetParametersFromConnectionStringCliMenuCommand(string dbServerName, IParametersManager parametersManager,
        Func<string?> inputConnectionString) : base("Set Parameters From Connection String", EMenuAction.Reload)
    {
        _dbServerName = dbServerName;
        _parametersManager = parametersManager;
        _inputConnectionString = inputConnectionString;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var parameters = (IParametersWithDatabaseServerConnections)_parametersManager.Parameters;
        if (!parameters.DatabaseServerConnections.TryGetValue(_dbServerName,
                out DatabaseServerConnectionData? databaseServerConnection))
        {
            StShared.WriteErrorLine($"Database Server Connection {_dbServerName} not found", true);
            return false;
        }

        string? connectionString = _inputConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            StShared.WriteErrorLine("Connection String is empty", true);
            return false;
        }

        string? error =
            DatabaseServerConnectionStringConverter.ApplyConnectionString(databaseServerConnection, connectionString);
        if (error is not null)
        {
            StShared.WriteErrorLine(error, true);
            return false;
        }

        return await _parametersManager.Save(_parametersManager.Parameters,
            $"Database Server Connection {_dbServerName} parameters set from Connection String", null,
            cancellationToken);
    }

    private static string? InputConnectionString()
    {
        var connectionStringInput = new TextDataInput("Connection String");
        return connectionStringInput.DoInput() ? connectionStringInput.Text : null;
    }
}
