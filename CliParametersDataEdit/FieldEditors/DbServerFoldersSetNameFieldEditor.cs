using System;
using System.Net.Http;
using System.Threading;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParameters.FieldEditors;
using CliParametersApiClientsEdit;
using CliParametersDataEdit.Cruders;
using DatabasesManagement;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibDataInput;
using LibMenuInput;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParametersDataEdit.FieldEditors;

public sealed class DbServerFoldersSetNameFieldEditor : FieldEditor<string>
{
    private readonly string _databaseConnectionNamePropertyName;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    public DbServerFoldersSetNameFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory, string propertyName,
        IParametersManager parametersManager, string databaseConnectionNamePropertyName) : base(propertyName)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _databaseConnectionNamePropertyName = databaseConnectionNamePropertyName;
        //_canUseNewDatabaseName = canUseNewDatabaseName;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        var currentFoldersSetName = GetValue(recordForUpdate);
        var databaseServerConnectionName = GetValue<string>(recordForUpdate, _databaseConnectionNamePropertyName);


        var databaseServerConnectionCruder =
            new DatabaseServerConnectionCruder(_logger, _httpClientFactory, _parametersManager);


        var databaseServerConnectionData = string.IsNullOrWhiteSpace(databaseServerConnectionName)
            ? null
            : (DatabaseServerConnectionData?)databaseServerConnectionCruder.GetItemByName(databaseServerConnectionName);

        if (databaseServerConnectionData == null)
            return;

        var databaseFoldersSets = databaseServerConnectionData.DatabaseFoldersSets;

        //var dataProvider = GetValue<EDatabaseProvider>(recordForUpdate, _dataProviderPropertyName);
        IDatabaseManager? databaseManager = null;

        switch (databaseServerConnectionData.DatabaseServerProvider)
        {
            case EDatabaseProvider.None:
            case EDatabaseProvider.SqLite:
            case EDatabaseProvider.OleDb:
                return;
            case EDatabaseProvider.SqlServer:


                break;
            case EDatabaseProvider.WebAgent:
                var databaseApiClientName = databaseServerConnectionData.DbWebAgentName;
                ApiClientCruder apiClientCruder = new(_parametersManager, _logger, _httpClientFactory);

                var apiClientSettings = string.IsNullOrWhiteSpace(databaseApiClientName)
                    ? null
                    : (ApiClientSettings?)apiClientCruder.GetItemByName(databaseApiClientName);

                if (databaseManager == null && apiClientSettings != null)
                    databaseManager = DatabaseAgentClientsFabric.CreateDatabaseManager(_logger, _httpClientFactory,
                            apiClientSettings, null, null, true, CancellationToken.None).Preserve().GetAwaiter()
                        .GetResult();

                if (databaseManager is not null)
                {
                    var getDatabaseFoldersSetsResult =
                        databaseManager.GetDatabaseFoldersSets(CancellationToken.None).Result;
                    if (getDatabaseFoldersSetsResult.IsT0)
                        databaseFoldersSets = getDatabaseFoldersSetsResult.AsT0;
                    else
                        Err.PrintErrorsOnConsole(getDatabaseFoldersSetsResult.AsT1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        CliMenuSet databasesMenuSet = new();

        foreach (var listItem in databaseFoldersSets)
            databasesMenuSet.AddMenuItem(
                new MenuCommandWithStatusCliMenuCommand(listItem.Key, listItem.Value.GetStatus()));

        var selectedKey = MenuInputer.InputFromMenuList(FieldName, databasesMenuSet, currentFoldersSetName);

        if (selectedKey is null)
            throw new DataInputException("Selected invalid Item. ");

        SetValue(recordForUpdate, selectedKey);
    }

    public override string GetValueStatus(object? record)
    {
        return GetValue(record) ?? string.Empty;
    }
}