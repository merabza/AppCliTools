using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using CliMenu;
using CliParameters;
using CliParameters.FieldEditors;
using CliParametersApiClientsEdit.FieldEditors;
using CliParametersDataEdit.CliMenuCommands;
using CliParametersDataEdit.FieldEditors;
using DatabasesManagement;
using DbTools.Models;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace CliParametersDataEdit.Cruders;

public sealed class DatabaseServerConnectionCruder : ParCruder<DatabaseServerConnectionData>
{
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly ILogger _logger;

    //public კონსტრუქტორი საჭიროა. გამოიყენება რეფლექსიით DictionaryFieldEditor-ში
    // ReSharper disable once MemberCanBePrivate.Global
    public DatabaseServerConnectionCruder(ILogger logger, IHttpClientFactory? httpClientFactory,
        IParametersManager parametersManager,
        Dictionary<string, DatabaseServerConnectionData> currentValuesDictionary) : base(parametersManager,
        currentValuesDictionary, "Database Server Connection", "Database Server Connections")
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        FieldEditors.Add(new EnumFieldEditor<EDatabaseProvider>(
            nameof(DatabaseServerConnectionData.DatabaseServerProvider), EDatabaseProvider.None));

        //მონაცემთა ბაზასთან დამაკავშირებელი ვებაგენტის სახელი
        if (httpClientFactory is not null)
        {
            FieldEditors.Add(new ApiClientNameFieldEditor(nameof(DatabaseServerConnectionData.DbWebAgentName), logger,
                httpClientFactory, ParametersManager, true));

            FieldEditors.Add(new RemoteDbConnectionNameFieldEditor(logger, httpClientFactory,
                nameof(DatabaseServerConnectionData.RemoteDbConnectionName), ParametersManager,
                nameof(DatabaseServerConnectionData.DbWebAgentName)));
        }

        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.ServerAddress)));
        FieldEditors.Add(new BoolFieldEditor(nameof(DatabaseServerConnectionData.WindowsNtIntegratedSecurity)));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.ServerUser)));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.ServerPass), null, false,
            ParametersEditor.PasswordChar));
        FieldEditors.Add(new BoolFieldEditor(nameof(DatabaseServerConnectionData.TrustServerCertificate), true));
        FieldEditors.Add(new BoolFieldEditor(nameof(DatabaseServerConnectionData.Encrypt)));
        FieldEditors.Add(new IntFieldEditor(nameof(DatabaseServerConnectionData.ConnectionTimeOut), 15));
        //FieldEditors.Add(new DatabaseBackupParametersFieldEditor(logger,
        //    nameof(DatabaseServerConnectionData.FullDbBackupParameters), parametersManager));
        //FieldEditors.Add(new DatabaseFoldersSetFieldEditor(parametersManager,
        //    nameof(DatabaseServerConnectionData.DatabaseFoldersSets)));

        FieldEditors.Add(new DictionaryFieldEditor<DatabaseFoldersSetCruder, DatabaseFoldersSet>(
            nameof(DatabaseServerConnectionData.DatabaseFoldersSets), parametersManager));
    }

    public static DatabaseServerConnectionCruder Create(ILogger logger, IHttpClientFactory? httpClientFactory,
        IParametersManager parametersManager)
    {
        var parameters = (IParametersWithDatabaseServerConnections)parametersManager.Parameters;
        return new DatabaseServerConnectionCruder(logger, httpClientFactory, parametersManager,
            parameters.DatabaseServerConnections);
    }

    public override bool CheckValidation(ItemData item)
    {
        try
        {
            var databaseServerConnectionData = GetTItem(item);

            var acParameters = (IParametersWithApiClients)ParametersManager.Parameters;
            var apiClients = new ApiClients(acParameters.ApiClients);

            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            token.ThrowIfCancellationRequested();

            var createDatabaseManagerResult = DatabaseManagersFactory.CreateDatabaseManager(_logger, true,
                databaseServerConnectionData, apiClients, _httpClientFactory, null, null, token).Preserve().Result;

            if (createDatabaseManagerResult.IsT1)
            {
                Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
                StShared.WriteErrorLine("dbManager could not created", true);
                return false;
            }

            Console.WriteLine("Try connect to server...");

            var dbManager = createDatabaseManagerResult.AsT0;

            var dbmTestConnectionResult = dbManager.TestConnection(null, token).Result;
            if (dbmTestConnectionResult.IsSome)
            {
                Err.PrintErrorsOnConsole((Err[])dbmTestConnectionResult);
                return false;
            }

            if (dbManager is RemoteDatabaseManager)
                return true;

            //თუ დაკავშირება მოხერხდა, მაშინ დადგინდეს სერვერის მხარეს შემდეგი პარამეტრები:
            //ბექაპირების ფოლდერი, ბაზის აღდგენის ფოლდერი, ბაზის ლოგების ფაილის აღდგენის ფოლდერი.
            var getDbServerInfoResult = dbManager.GetDatabaseServerInfo(token).Result;
            if (getDbServerInfoResult.IsT1)
            {
                Err.PrintErrorsOnConsole(getDbServerInfoResult.AsT1);
                return false;
            }

            var dbServerInfo = getDbServerInfoResult.AsT0;

            Console.WriteLine($"Server Name is {dbServerInfo.ServerName}");
            Console.WriteLine(
                $"Server is {(dbServerInfo.AllowsCompression ? string.Empty : "NOT ")} Allows Compression");
            var isServerLocalResult = dbManager.IsServerLocal(token).Result;
            Console.WriteLine(isServerLocalResult.IsT0
                ? $"Server is {(isServerLocalResult.AsT0 ? string.Empty : "NOT ")} local"
                : "Server is local or not is not detected");

            Console.WriteLine($"Server Product Version is {dbServerInfo.ServerProductVersion}");
            Console.WriteLine($"Server Instance Name is {dbServerInfo.ServerInstanceName}");
            Console.WriteLine($"Backup Directory is {dbServerInfo.BackupDirectory}");
            Console.WriteLine($"Default Data Directory is {dbServerInfo.DefaultDataDirectory}");
            Console.WriteLine($"Default Log Directory is {dbServerInfo.DefaultLogDirectory}");

            databaseServerConnectionData.SetDefaultFolders(dbServerInfo);

            return true;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return false;
    }

    protected override void CheckFieldsEnables(ItemData itemData, string? lastEditedFieldName = null)
    {
        var databaseServerConnection = (DatabaseServerConnectionData)itemData;
        var enableUser = false;
        var enablePassword = false;
        var enableSqlServerProps = false;
        var enableWebAgentProps = false;

        switch (databaseServerConnection.DatabaseServerProvider)
        {
            case EDatabaseProvider.None:
                EnableAllFieldButOne(nameof(DatabaseServerConnectionData.DatabaseServerProvider), false);
                return;
            case EDatabaseProvider.SqlServer:
                enablePassword = enableUser = !databaseServerConnection.WindowsNtIntegratedSecurity;
                enableSqlServerProps = true;
                break;
            case EDatabaseProvider.SqLite:
            case EDatabaseProvider.OleDb:
                enablePassword = true;
                break;
            case EDatabaseProvider.WebAgent:
                enableWebAgentProps = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        //EnableFieldByName(nameof(DatabaseServerConnectionData.DatabaseServerProvider));

        EnableFieldByName(nameof(DatabaseServerConnectionData.DbWebAgentName), enableWebAgentProps);
        EnableFieldByName(nameof(DatabaseServerConnectionData.RemoteDbConnectionName), enableWebAgentProps);
        //EnableFieldByName(nameof(DatabaseServerConnectionData.FullDbBackupParameters), !enableWebAgentProps);
        EnableFieldByName(nameof(DatabaseServerConnectionData.DatabaseFoldersSets), !enableWebAgentProps);

        EnableFieldByName(nameof(DatabaseServerConnectionData.ServerAddress), enableSqlServerProps);
        EnableFieldByName(nameof(DatabaseServerConnectionData.WindowsNtIntegratedSecurity), enableSqlServerProps);

        EnableFieldByName(nameof(DatabaseServerConnectionData.ServerUser), enableUser);

        EnableFieldByName(nameof(DatabaseServerConnectionData.ServerPass), enablePassword);

        EnableFieldByName(nameof(DatabaseServerConnectionData.TrustServerCertificate), enableSqlServerProps);
        EnableFieldByName(nameof(DatabaseServerConnectionData.ConnectionTimeOut), enableSqlServerProps);
        EnableFieldByName(nameof(DatabaseServerConnectionData.Encrypt), enableSqlServerProps);

        if (lastEditedFieldName != nameof(DatabaseServerConnectionData.ServerUser) &&
            lastEditedFieldName != nameof(DatabaseServerConnectionData.ServerPass) &&
            lastEditedFieldName != nameof(DatabaseServerConnectionData.ServerAddress))
            return;

        if (!string.IsNullOrWhiteSpace(databaseServerConnection.ServerAddress) &&
            !string.IsNullOrWhiteSpace(databaseServerConnection.ServerUser) &&
            !string.IsNullOrWhiteSpace(databaseServerConnection.ServerPass))
            CheckValidation(itemData);
    }

    public override string GetStatusFor(string name)
    {
        var databaseServerConnection = (DatabaseServerConnectionData?)GetItemByName(name);
        return
            $"{databaseServerConnection?.DatabaseServerProvider.ToString()}: {databaseServerConnection?.ServerAddress}";
    }

    public override void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string recordKey)
    {
        base.FillDetailsSubMenu(itemSubMenuSet, recordKey);

        var parameters = (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        var databaseServerConnectionDataByKey = parameters.DatabaseServerConnections[recordKey];

        if (databaseServerConnectionDataByKey.DatabaseServerProvider == EDatabaseProvider.WebAgent)
            return;

        var getDbServerFoldersCliMenuCommand =
            new GetDbServerFoldersCliMenuCommand(_logger, _httpClientFactory, recordKey, ParametersManager);
        itemSubMenuSet.AddMenuItem(getDbServerFoldersCliMenuCommand);
    }
}