using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DatabasesManagement;
using LibApiClientParameters;
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
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor

    public GetDbServerFoldersToolAction(ILogger logger, IHttpClientFactory httpClientFactory, string dbServerName,
        IParametersManager parametersManager) : base(logger, ActionName, null, null, true)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _dbServerName = dbServerName;
        _parametersManager = parametersManager;
    }

    protected override async Task<bool> RunAction(CancellationToken cancellationToken = default)
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

        var createDatabaseManagerResult = await DatabaseManagersFabric.CreateDatabaseManager(_logger,
            _httpClientFactory, true, _dbServerName, databaseServerConnections, apiClients, null, null,
            CancellationToken.None);

        if (createDatabaseManagerResult.IsT1)
        {
            Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
            StShared.WriteErrorLine("Database Management Clients could not created", true, _logger);
            return false;
        }

        var getDatabaseServerInfoResult =
            await createDatabaseManagerResult.AsT0.GetDatabaseServerInfo(cancellationToken);
        if (getDatabaseServerInfoResult.IsT1)
        {
            Err.PrintErrorsOnConsole(getDatabaseServerInfoResult.AsT1);
            return false;
        }

        var dbInfo = getDatabaseServerInfoResult.AsT0;

        var dbCon = parameters.DatabaseServerConnections[_dbServerName];

        dbCon.SetDefaultFolders(dbInfo);

        _parametersManager.Save(parameters, "folders Changed and saved");

        return true;
    }
}