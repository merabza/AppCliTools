//using System.Threading;
//using System.Threading.Tasks;
//using DatabasesManagement;
//using LibDatabaseParameters;
//using LibParameters;
//using LibToolActions;
//using Microsoft.Extensions.Logging;
//using SystemToolsShared;
//using SystemToolsShared.Errors;

//namespace CliParametersDataEdit.ToolActions;

//public class PutDbServerFoldersToolAction : ToolAction
//{
//    private const string ActionName = "Put Database Server Folders and save in parameters";

//    //public const string ActionDescription = "Put Database Server Folders and save in parameters";
//    private readonly string _dbServerName;
//    private readonly ILogger _logger;
//    private readonly IParametersManager _parametersManager;

//    // ReSharper disable once ConvertToPrimaryConstructor
//    public PutDbServerFoldersToolAction(ILogger logger, string dbServerName, IParametersManager parametersManager) :
//        base(logger, ActionName, null, null, true)
//    {
//        _logger = logger;
//        _dbServerName = dbServerName;
//        _parametersManager = parametersManager;
//    }


//    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
//    {
//        var parameters = (IParametersWithDatabaseServerConnections)_parametersManager.Parameters;

//        DatabaseServerConnections databaseServerConnections = new(parameters.DatabaseServerConnections);

//        if (string.IsNullOrWhiteSpace(_dbServerName))
//        {
//            StShared.WriteErrorLine("Database server name is not specified", true, _logger);
//            return false;
//        }

//        var databaseManagementClient = await DatabaseAgentClientsFabric.CreateDatabaseManager(true, _logger,
//            _dbServerName, databaseServerConnections, null, null, CancellationToken.None);

//        if (databaseManagementClient is null)
//        {
//            StShared.WriteErrorLine("Database Management Clients could not created", true, _logger);
//            return false;
//        }

//        var dbCon = parameters.DatabaseServerConnections[_dbServerName];

//        if (string.IsNullOrWhiteSpace(dbCon.BackupFolderName))
//        {
//            StShared.WriteErrorLine("Backup Folder Name is not specified", true, _logger);
//            return false;
//        }

//        if (string.IsNullOrWhiteSpace(dbCon.DataFolderName))
//        {
//            StShared.WriteErrorLine("Data Folder Name is not specified", true, _logger);
//            return false;
//        }

//        if (string.IsNullOrWhiteSpace(dbCon.DataLogFolderName))
//        {
//            StShared.WriteErrorLine("Data Log Folder Name is not specified", true, _logger);
//            return false;
//        }

//        var getDatabaseServerInfoResult = await databaseManagementClient.SetDefaultFolders(dbCon.BackupFolderName,
//            dbCon.DataFolderName, dbCon.DataLogFolderName, cancellationToken);
//        if (!getDatabaseServerInfoResult.IsSome)
//            return true;

//        Err.PrintErrorsOnConsole((Err[])getDatabaseServerInfoResult);
//        return false;
//    }
//}

