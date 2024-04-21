using LibToolActions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using DatabasesManagement;
using LibDatabaseParameters;
using SystemToolsShared;
using LibParameters;

namespace CliParametersDataEdit.ToolActions;

public class GetDbServerFoldersToolAction : ToolAction
{
    private readonly ILogger _logger;
    private readonly string _dbServerName;
    private readonly IParametersManager _parametersManager;
    private const string ActionName = "Get Database Server Folders and save in parameters";
    public const string ActionDescription = "Get Database Server Folders and save in parameters";

    //GetDbServerFoldersCliMenuCommand getDbServerFoldersCliMenuCommand = new(_logger, recordKey, ParametersManager);
    //itemSubMenuSet.AddMenuItem(getDbServerFoldersCliMenuCommand, "Get Database Server Folders and save in parameters");

    //PutDbServerFoldersCliMenuCommand putDbServerFoldersCliMenuCommand = new(_logger, recordKey, ParametersManager);
    //itemSubMenuSet.AddMenuItem(putDbServerFoldersCliMenuCommand, "Put Database Server Folders from parameters to server");


    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDbServerFoldersToolAction(ILogger logger, string dbServerName, IParametersManager parametersManager) :
        base(logger, ActionName, null, null, true)
    {
        _logger = logger;
        _dbServerName = dbServerName;
        _parametersManager = parametersManager;
    }


    protected override async Task<bool> RunAction(CancellationToken cancellationToken)
    {

        var parameters = (IParametersWithDatabaseServerConnections)_parametersManager.Parameters;

        DatabaseServerConnections databaseServerConnections = new(parameters.DatabaseServerConnections);

        if (string.IsNullOrWhiteSpace(_dbServerName))
        {
            StShared.WriteErrorLine("Database server name is not specified", true, _logger);
            return false;
        }

        var databaseManagementClient = await DatabaseAgentClientsFabric.CreateDatabaseManagementClient(true, _logger,
            _dbServerName, databaseServerConnections, null, null, CancellationToken.None);

        if ( databaseManagementClient is null)
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

        _parametersManager.Save(parameters,"folders Changed and saved");

        return true;
        //var getVersionSuccess = false;
        //var version = "";
        //var tryCount = 0;
        //while (!getVersionSuccess && tryCount < _maxTryCount)
        //{
        //    if (tryCount > 0)
        //    {
        //        _logger.LogInformation("waiting for 3 second...");
        //        Thread.Sleep(3000);
        //    }

        //    tryCount++;
        //    try
        //    {
        //        _logger.LogInformation("Try to get parameters Version {tryCount}...", tryCount);

        //        var errors = new List<Err>();

        //        if (_proxySettings is ProxySettings proxySettings)
        //        {
        //            //კლიენტის შექმნა ვერსიის შესამოწმებლად
        //            // ReSharper disable once using
        //            // ReSharper disable once DisposableConstructor
        //            await using var proxyApiClient = new ProjectsProxyApiClient(_logger, _webAgentForCheck.Server,
        //                _webAgentForCheck.ApiKey, _webAgentForCheck.WithMessaging);
        //            var getAppSettingsVersionByProxyResult =
        //                await proxyApiClient.GetAppSettingsVersionByProxy(proxySettings.ServerSidePort,
        //                    proxySettings.ApiVersionId, cancellationToken);
        //            if (getAppSettingsVersionByProxyResult.IsT1)
        //                errors.AddRange(getAppSettingsVersionByProxyResult.AsT1);
        //            else
        //                version = getAppSettingsVersionByProxyResult.AsT0;
        //        }
        //        else
        //        {
        //            //კლიენტის შექმნა ვერსიის შესამოწმებლად
        //            // ReSharper disable once using
        //            // ReSharper disable once DisposableConstructor
        //            await using var testApiClient = new TestApiClient(_logger, _webAgentForCheck.Server);
        //            var getAppSettingsVersionResult = await testApiClient.GetAppSettingsVersion(cancellationToken);
        //            if (getAppSettingsVersionResult.IsT1)
        //                errors.AddRange(getAppSettingsVersionResult.AsT1);
        //            else
        //                version = getAppSettingsVersionResult.AsT0;
        //        }

        //        if (errors.Count > 0)
        //            Err.PrintErrorsOnConsole(errors);
        //        else
        //        {

        //            getVersionSuccess = true;

        //            if (_appSettingsVersion == null)
        //            {
        //                _logger.LogInformation("{ProjectName} is running on parameters version {version}", ProjectName,
        //                    version);
        //                return true;
        //            }

        //            if (_appSettingsVersion != version)
        //            {
        //                _logger.LogWarning("Current Parameters version is {version}, but must be {_appSettingsVersion}",
        //                    version, _appSettingsVersion);
        //                getVersionSuccess = false;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        _logger.LogWarning("could not get Parameters version for project {ProjectName} on try {tryCount}",
        //            ProjectName, tryCount);
        //    }
        //}

        //if (!getVersionSuccess)
        //{
        //    _logger.LogError("could not get parameters version for project {ProjectName}", ProjectName);
        //    return false;
        //}

        //if (_appSettingsVersion != version)
        //{
        //    _logger.LogError("Current parameters version is {version}, but must be {_appSettingsVersion}", version,
        //        _appSettingsVersion);
        //    return false;
        //}

        //_logger.LogInformation("{ProjectName} now is running on parameters version {version}", ProjectName, version);
        //return true;
    }


}