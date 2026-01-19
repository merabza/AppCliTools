using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using CliMenu;
using CliParameters.FieldEditors;
using LibDataInput;
using LibMenuInput;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;
using WebAgentContracts.WebAgentDatabasesApiContracts;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParametersDataEdit.FieldEditors;

public sealed class RemoteDbConnectionNameFieldEditor : FieldEditor<string>
{
    private readonly string _databaseApiClientNameFieldName;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    public RemoteDbConnectionNameFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory, string propertyName,
        IParametersManager parametersManager, string databaseApiClientNameFieldName) : base(propertyName)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _databaseApiClientNameFieldName = databaseApiClientNameFieldName;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        try
        {
            string? currentRemoteDbConnectionName = GetValue(recordForUpdate);
            string? databaseApiClientName = GetValue<string>(recordForUpdate, _databaseApiClientNameFieldName);
            var acParameters = (IParametersWithApiClients)_parametersManager.Parameters;
            var apiClients = new ApiClients(acParameters.ApiClients);

            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            token.ThrowIfCancellationRequested();
            OneOf<IDatabaseManager, Err[]> createDatabaseManagerResult = DatabaseManagersFactory
                .CreateRemoteDatabaseManager(_logger, _httpClientFactory, true, databaseApiClientName, apiClients, null,
                    null, token).Result;

            var databaseConnectionNames = new List<string>();
            if (createDatabaseManagerResult.IsT1)
            {
                Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
            }
            else
            {
                DatabaseApiClient apiClient = ((RemoteDatabaseManager)createDatabaseManagerResult.AsT0).ApiClient;
                OneOf<List<string>, Err[]> getDatabaseFoldersSetsResult =
                    apiClient.GetDatabaseConnectionNames(token).Result;
                if (getDatabaseFoldersSetsResult.IsT0)
                {
                    databaseConnectionNames = getDatabaseFoldersSetsResult.AsT0;
                }
                else
                {
                    Err.PrintErrorsOnConsole(getDatabaseFoldersSetsResult.AsT1);
                }
            }

            var databasesMenuSet = new CliMenuSet();

            foreach (string listItem in databaseConnectionNames)
            {
                databasesMenuSet.AddMenuItem(new CliMenuCommand(listItem));
            }

            string selectedKey =
                MenuInputer.InputFromMenuList(FieldName, databasesMenuSet, currentRemoteDbConnectionName) ??
                throw new DataInputException("Selected invalid Item. ");

            SetValue(recordForUpdate, selectedKey);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
        catch (Exception e)
        {
            string contextualMessage =
                $"Error in RemoteDbConnectionNameFieldEditor.UpdateField for recordKey: {recordKey}, property: {FieldName}";
            _logger.LogError(e,
                "Error in RemoteDbConnectionNameFieldEditor.UpdateField for recordKey: {RecordKey}, property: {FieldName}",
                recordKey, FieldName);
            throw new Exception(contextualMessage, e);
        }
    }

    public override string GetValueStatus(object? record)
    {
        return GetValue(record) ?? string.Empty;
    }
}
