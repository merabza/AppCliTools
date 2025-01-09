using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParameters.FieldEditors;
using CliParametersApiClientsEdit;
using CliParametersDataEdit.Cruders;
using DatabasesManagement;
using DbTools;
using DbTools.Models;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibDataInput;
using LibMenuInput;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParametersApiClientsDbEdit;

public sealed class DbServerFoldersSetNameFieldEditor : FieldEditor<string>
{
    //private readonly bool _canUseNewDatabaseName;
    private readonly string _databaseApiClientNameFieldName;
    private readonly string _databaseConnectionNamePropertyName;
    private readonly string _dataProviderPropertyName;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    public DbServerFoldersSetNameFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory, string propertyName,
        IParametersManager parametersManager, string dataProviderPropertyName,
        string databaseConnectionNamePropertyName, string databaseApiClientNameFieldName) : base(propertyName)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _databaseConnectionNamePropertyName = databaseConnectionNamePropertyName;
        _databaseApiClientNameFieldName = databaseApiClientNameFieldName;
        _dataProviderPropertyName = dataProviderPropertyName;
        //_canUseNewDatabaseName = canUseNewDatabaseName;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        var currentFoldersSetName = GetValue(recordForUpdate);
        var databaseFoldersSets = new Dictionary<string, DatabaseFoldersSet>();

        var dataProvider = GetValue<EDataProvider>(recordForUpdate, _dataProviderPropertyName);
        IDatabaseManager? databaseManager = null;

        switch (dataProvider)
        {
            case EDataProvider.None:
            case EDataProvider.SqLite:
            case EDataProvider.OleDb:
                return;
            case EDataProvider.Sql:
                var databaseServerConnectionName =
                    GetValue<string>(recordForUpdate, _databaseConnectionNamePropertyName);
                DatabaseServerConnectionCruder databaseServerConnectionCruder = new(_parametersManager, _logger);

                var databaseServerConnectionData = string.IsNullOrWhiteSpace(databaseServerConnectionName)
                    ? null
                    : (DatabaseServerConnectionData?)databaseServerConnectionCruder.GetItemByName(
                        databaseServerConnectionName);

                if (databaseServerConnectionData != null)
                    databaseFoldersSets = databaseServerConnectionData.DatabaseFoldersSets;

                break;
            case EDataProvider.WebAgent:
                var databaseApiClientName = GetValue<string>(recordForUpdate, _databaseApiClientNameFieldName);
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