using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DatabaseTools.DbTools.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.BackgroundTasks;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using ToolsManagement.DatabasesManagement;

namespace AppCliTools.CliParametersDataEdit.ToolActions;

public sealed class GetDbServerFoldersToolAction : ToolAction
{
    private const string ActionName = "Get Database Server Folders and save in parameters";
    private readonly string _appName;

    private readonly string _dbServerName;
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDbServerFoldersToolAction(string appName, ILogger logger, IHttpClientFactory? httpClientFactory,
        string dbServerName, IParametersManager parametersManager) : base(logger, ActionName, null, null, true)
    {
        _appName = appName;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _dbServerName = dbServerName;
        _parametersManager = parametersManager;
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_dbServerName))
        {
            StShared.WriteErrorLine("Database server name is not specified", true, _logger);
            return false;
        }

        var parameters = (IParametersWithDatabaseServerConnections)_parametersManager.Parameters;
        var databaseServerConnections = new DatabaseServerConnections(parameters.DatabaseServerConnections);
        var acParameters = (IParametersWithApiClients)_parametersManager.Parameters;
        var apiClients = new ApiClients(acParameters.ApiClients);

        Result<IDatabaseManager> createDatabaseManagerResult =
            await DatabaseManagersFactory.CreateDatabaseManager(_appName, _logger, true, _dbServerName,
                databaseServerConnections, apiClients, _httpClientFactory, null, null, cancellationToken);

        if (createDatabaseManagerResult.IsFailure)
        {
            createDatabaseManagerResult.Error.PrintErrorsOnConsole();
            StShared.WriteErrorLine("Database Management Clients could not created", true, _logger);
            return false;
        }

        Result<DbServerInfo> getDatabaseServerInfoResult =
            await createDatabaseManagerResult.Value.GetDatabaseServerInfo(cancellationToken);
        if (getDatabaseServerInfoResult.IsFailure)
        {
            getDatabaseServerInfoResult.Error.PrintErrorsOnConsole();
            return false;
        }

        DbServerInfo dbInfo = getDatabaseServerInfoResult.Value;

        DatabaseServerConnectionData dbCon = parameters.DatabaseServerConnections[_dbServerName];

        dbCon.SetDefaultFolders(dbInfo);

        await _parametersManager.Save(parameters, "folders Changed and saved", null, cancellationToken);

        return true;
    }
}
