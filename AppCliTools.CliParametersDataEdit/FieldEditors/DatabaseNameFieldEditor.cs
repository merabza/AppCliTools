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
using DatabaseTools.DbTools.Models;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;

namespace AppCliTools.CliParametersDataEdit.FieldEditors;

public sealed class DatabaseNameFieldEditor : FieldEditor<string>
{
    private readonly bool _canUseNewDatabaseName;
    private readonly string _databaseConnectionNamePropertyName;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DatabaseNameFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory, string propertyName,
        IParametersManager parametersManager, string databaseConnectionNamePropertyName,
        bool canUseNewDatabaseName) : base(propertyName)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _databaseConnectionNamePropertyName = databaseConnectionNamePropertyName;
        _canUseNewDatabaseName = canUseNewDatabaseName;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        try
        {
            string? currentDatabaseName = GetValue(recordForUpdate);
            string? databaseServerConnectionName =
                GetValue<string>(recordForUpdate, _databaseConnectionNamePropertyName);
            var dscParameters = (IParametersWithDatabaseServerConnections)_parametersManager.Parameters;
            var databaseServerConnections = new DatabaseServerConnections(dscParameters.DatabaseServerConnections);
            var acParameters = (IParametersWithApiClients)_parametersManager.Parameters;
            var apiClients = new ApiClients(acParameters.ApiClients);
            var databaseInfos = new List<DatabaseInfoModel>();

            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            token.ThrowIfCancellationRequested();

            OneOf<IDatabaseManager, Err[]> createDatabaseManagerResult = DatabaseManagersFactory
                .CreateDatabaseManager(_logger, true, databaseServerConnectionName, databaseServerConnections,
                    apiClients, _httpClientFactory, null, null, token).Result;

            if (createDatabaseManagerResult.IsT1)
            {
                Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
                return;
            }

            OneOf<List<DatabaseInfoModel>, Err[]> getDatabaseNamesResult =
                createDatabaseManagerResult.AsT0.GetDatabaseNames(token).Result;
            if (getDatabaseNamesResult.IsT0)
            {
                databaseInfos = getDatabaseNamesResult.AsT0;
            }
            else
            {
                Err.PrintErrorsOnConsole(getDatabaseNamesResult.AsT1);
            }

            var databasesMenuSet = new CliMenuSet();
            if (_canUseNewDatabaseName)
            {
                databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand("New Database Name"));
            }

            List<string> keys = databaseInfos.Select(s => s.Name).ToList();
            foreach (string listItem in keys)
            {
                databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand(listItem));
            }

            int selectedId = MenuInputer.InputIdFromMenuList(FieldName, databasesMenuSet, currentDatabaseName);

            if (_canUseNewDatabaseName && selectedId == 0)
            {
                string newDatabaseName = Inputer.InputTextRequired("New Database Name"); // nameInput.Text;
                SetValue(recordForUpdate, newDatabaseName);
                return;
            }

            int index = selectedId - (_canUseNewDatabaseName ? 1 : 0);
            if (index < 0 || index >= keys.Count)
            {
                throw new DataInputException("Selected invalid ID. ");
            }

            SetValue(recordForUpdate, keys[index]);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public override string GetValueStatus(object? record)
    {
        return GetValue(record) ?? string.Empty;
    }
}
