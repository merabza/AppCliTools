﻿using System.Linq;
using System.Net.Http;
using System.Threading;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParameters.FieldEditors;
using DatabasesManagement;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibDataInput;
using LibMenuInput;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;
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
        var dscParameters = (IParametersWithDatabaseServerConnections)_parametersManager.Parameters;
        var databaseServerConnections = new DatabaseServerConnections(dscParameters.DatabaseServerConnections);
        var acParameters = (IParametersWithApiClients)_parametersManager.Parameters;
        var apiClients = new ApiClients(acParameters.ApiClients);

        if (string.IsNullOrWhiteSpace(databaseServerConnectionName))
        {
            StShared.WriteErrorLine("databaseServerConnectionName is not specified", true, _logger);
            return;
        }

        var databaseServerConnectionData =
            databaseServerConnections.GetDatabaseServerConnectionByKey(databaseServerConnectionName);

        if (databaseServerConnectionData == null)
        {
            StShared.WriteErrorLine("databaseServerConnectionData is not Created", true, _logger);
            return;
        }

        var createDatabaseManagerResult = DatabaseManagersFactory.CreateDatabaseManager(_logger, true,
                databaseServerConnectionData, apiClients, _httpClientFactory, null, null, CancellationToken.None)
            .Preserve()
            .Result;
        var databaseFoldersSetNames = databaseServerConnectionData.DatabaseFoldersSets.Keys.ToList();

        if (createDatabaseManagerResult.IsT1)
        {
            Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
        }
        else
        {
            var getDatabaseFoldersSetsResult = createDatabaseManagerResult.AsT0
                .GetDatabaseFoldersSetNames(CancellationToken.None).Result;
            if (getDatabaseFoldersSetsResult.IsT0)
                databaseFoldersSetNames = getDatabaseFoldersSetsResult.AsT0;
            else
                Err.PrintErrorsOnConsole(getDatabaseFoldersSetsResult.AsT1);
        }

        var databasesMenuSet = new CliMenuSet();

        foreach (var listItem in databaseFoldersSetNames)
            databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand(listItem));

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