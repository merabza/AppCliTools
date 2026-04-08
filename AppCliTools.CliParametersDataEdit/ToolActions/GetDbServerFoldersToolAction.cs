using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DatabaseTools.DbTools.Models;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.BackgroundTasks;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;

namespace AppCliTools.CliParametersDataEdit.ToolActions;

public sealed class GetDbServerFoldersToolAction : ToolAction
{
    private const string ActionName = "Get Database Server Folders and save in parameters";
    private readonly string _appName;

    //public const string ActionDescription = "Get Database Server Folders and save in parameters";
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

        OneOf<IDatabaseManager, Error[]> createDatabaseManagerResult =
            await DatabaseManagersFactory.CreateDatabaseManager(_appName, _logger, true, _dbServerName,
                databaseServerConnections, apiClients, _httpClientFactory, null, null, cancellationToken);

        if (createDatabaseManagerResult.IsT1)
        {
            Error.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
            StShared.WriteErrorLine("Database Management Clients could not created", true, _logger);
            return false;
        }

        OneOf<DbServerInfo, Error[]> getDatabaseServerInfoResult =
            await createDatabaseManagerResult.AsT0.GetDatabaseServerInfo(cancellationToken);
        if (getDatabaseServerInfoResult.IsT1)
        {
            Error.PrintErrorsOnConsole(getDatabaseServerInfoResult.AsT1);
            return false;
        }

        DbServerInfo? dbInfo = getDatabaseServerInfoResult.AsT0;

        DatabaseServerConnectionData dbCon = parameters.DatabaseServerConnections[_dbServerName];

        dbCon.SetDefaultFolders(dbInfo);

        await _parametersManager.Save(parameters, "folders Changed and saved", null, cancellationToken);

        return true;
    }
}
