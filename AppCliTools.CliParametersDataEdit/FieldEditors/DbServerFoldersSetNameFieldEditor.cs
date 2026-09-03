using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.LibDataInput;
using AppCliTools.LibMenuInput;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using ToolsManagement.DatabasesManagement;

// ReSharper disable ConvertToPrimaryConstructor

namespace AppCliTools.CliParametersDataEdit.FieldEditors;

public sealed class DbServerFoldersSetNameFieldEditor : FieldEditor<string>
{
    private readonly string _appName;
    private readonly string _databaseConnectionNamePropertyName;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    public DbServerFoldersSetNameFieldEditor(string appName, ILogger logger, IHttpClientFactory httpClientFactory,
        string propertyName, IParametersManager parametersManager,
        string databaseConnectionNamePropertyName) : base(propertyName)
    {
        _appName = appName;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _databaseConnectionNamePropertyName = databaseConnectionNamePropertyName;
        //_canUseNewDatabaseName = canUseNewDatabaseName;
    }

    public override async ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string? currentFoldersSetName = GetValue(recordForUpdate);
            string? databaseServerConnectionName =
                GetValue<string>(recordForUpdate, _databaseConnectionNamePropertyName);
            var dscParameters = (IParametersWithDatabaseServerConnections)_parametersManager.Parameters;
            var databaseServerConnections = new DatabaseServerConnections(dscParameters.DatabaseServerConnections);
            var acParameters = (IParametersWithApiClients)_parametersManager.Parameters;
            var apiClients = new ApiClients(acParameters.ApiClients);

            if (string.IsNullOrWhiteSpace(databaseServerConnectionName))
            {
                StShared.WriteErrorLine("databaseServerConnectionName is not specified", true, _logger);
                return;
            }

            DatabaseServerConnectionData? databaseServerConnectionData =
                databaseServerConnections.GetDatabaseServerConnectionByKey(databaseServerConnectionName);

            if (databaseServerConnectionData == null)
            {
                StShared.WriteErrorLine("databaseServerConnectionData is not Created", true, _logger);
                return;
            }

            Result<IDatabaseManager> createDatabaseManagerResult =
                await DatabaseManagersFactory.CreateDatabaseManager(_appName, _logger, true,
                    databaseServerConnectionData, apiClients, _httpClientFactory, null, null, cancellationToken);
            List<string> databaseFoldersSetNames =
                databaseServerConnectionData.DatabaseFoldersSets?.Keys.ToList() ?? [];

            if (createDatabaseManagerResult.IsFailure)
            {
                createDatabaseManagerResult.Error.PrintErrorsOnConsole();
            }
            else
            {
                Result<List<string>> getDatabaseFoldersSetsResult = await createDatabaseManagerResult.Value
                    .GetDatabaseFoldersSetNames(cancellationToken);
                if (getDatabaseFoldersSetsResult.IsSuccess)
                {
                    databaseFoldersSetNames = getDatabaseFoldersSetsResult.Value;
                }
                else
                {
                    getDatabaseFoldersSetsResult.Error.PrintErrorsOnConsole();
                }
            }

            var databasesMenuSet = new CliMenuSet();

            foreach (string listItem in databaseFoldersSetNames)
            {
                databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand(listItem));
            }

            string selectedKey = MenuInputer.InputFromMenuList(FieldName, databasesMenuSet, currentFoldersSetName) ??
                                 throw new DataInputException("Selected invalid Item. ");

            SetValue(recordForUpdate, selectedKey);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "ErrorOmd in DbServerFoldersSetNameFieldEditor.UpdateField for recordKey: {RecordKey}, property: {PropertyName}",
                recordKey, PropertyName);
            throw new Exception(
                $"ErrorOmd occurred in DbServerFoldersSetNameFieldEditor.UpdateField for recordKey: {recordKey}, property: {PropertyName}",
                e);
        }
    }

    public override string GetValueStatus(object? record)
    {
        return GetValue(record) ?? string.Empty;
    }
}
