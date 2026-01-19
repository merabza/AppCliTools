using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.LibDataInput;
using AppCliTools.LibMenuInput;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;

// ReSharper disable ConvertToPrimaryConstructor

namespace AppCliTools.CliParametersDataEdit.FieldEditors;

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

            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            token.ThrowIfCancellationRequested();
            OneOf<IDatabaseManager, Err[]> createDatabaseManagerResult = DatabaseManagersFactory
                .CreateDatabaseManager(_logger, true, databaseServerConnectionData, apiClients, _httpClientFactory,
                    null, null, token).Preserve().Result;
            List<string>? databaseFoldersSetNames =
                databaseServerConnectionData.DatabaseFoldersSets?.Keys.ToList() ?? [];

            if (createDatabaseManagerResult.IsT1)
            {
                Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
            }
            else
            {
                OneOf<List<string>, Err[]> getDatabaseFoldersSetsResult = createDatabaseManagerResult.AsT0
                    .GetDatabaseFoldersSetNames(token).Result;
                if (getDatabaseFoldersSetsResult.IsT0)
                {
                    databaseFoldersSetNames = getDatabaseFoldersSetsResult.AsT0;
                }
                else
                {
                    Err.PrintErrorsOnConsole(getDatabaseFoldersSetsResult.AsT1);
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
                "Error in DbServerFoldersSetNameFieldEditor.UpdateField for recordKey: {RecordKey}, property: {PropertyName}",
                recordKey, PropertyName);
            throw new Exception(
                $"Error occurred in DbServerFoldersSetNameFieldEditor.UpdateField for recordKey: {recordKey}, property: {PropertyName}",
                e);
        }
    }

    public override string GetValueStatus(object? record)
    {
        return GetValue(record) ?? string.Empty;
    }
}
