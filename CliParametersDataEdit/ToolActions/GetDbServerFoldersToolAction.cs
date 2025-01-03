﻿using System.Threading;
using System.Threading.Tasks;
using DatabasesManagement;
using LibDatabaseParameters;
using LibParameters;
using LibToolActions;
using Microsoft.Extensions.Logging;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace CliParametersDataEdit.ToolActions;

public class GetDbServerFoldersToolAction : ToolAction
{
    private const string ActionName = "Get Database Server Folders and save in parameters";

    //public const string ActionDescription = "Get Database Server Folders and save in parameters";
    private readonly string _dbServerName;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDbServerFoldersToolAction(ILogger logger, string dbServerName, IParametersManager parametersManager) :
        base(logger, ActionName, null, null, true)
    {
        _logger = logger;
        _dbServerName = dbServerName;
        _parametersManager = parametersManager;
    }


    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        var parameters = (IParametersWithDatabaseServerConnections)_parametersManager.Parameters;

        DatabaseServerConnections databaseServerConnections = new(parameters.DatabaseServerConnections);

        if (string.IsNullOrWhiteSpace(_dbServerName))
        {
            StShared.WriteErrorLine("Database server name is not specified", true, _logger);
            return false;
        }

        var databaseManagementClient = await DatabaseAgentClientsFabric.CreateDatabaseManager(true, _logger,
            _dbServerName, databaseServerConnections, null, null, CancellationToken.None);

        if (databaseManagementClient is null)
        {
            StShared.WriteErrorLine("Database Management Clients could not created", true, _logger);
            return false;
        }

        var getDatabaseServerInfoResult = await databaseManagementClient.GetDatabaseServerInfo(cancellationToken);
        if (getDatabaseServerInfoResult.IsT1)
        {
            Err.PrintErrorsOnConsole(getDatabaseServerInfoResult.AsT1);
            return false;
        }

        var dbInfo = getDatabaseServerInfoResult.AsT0;

        var dbCon = parameters.DatabaseServerConnections[_dbServerName];
        dbCon.BackupFolderName = dbInfo.BackupDirectory;
        dbCon.DataFolderName = dbInfo.DefaultDataDirectory;
        dbCon.DataLogFolderName = dbInfo.DefaultLogDirectory;

        _parametersManager.Save(parameters, "folders Changed and saved");

        return true;
    }
}