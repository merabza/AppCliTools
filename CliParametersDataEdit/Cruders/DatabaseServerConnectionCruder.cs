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
using DatabaseTools.DbTools.Models;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;

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
            DatabaseServerConnectionData databaseServerConnectionData = GetTItem(item);

            var acParameters = (IParametersWithApiClients)ParametersManager.Parameters;
            var apiClients = new ApiClients(acParameters.ApiClients);

            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            token.ThrowIfCancellationRequested();

            OneOf<IDatabaseManager, Err[]> createDatabaseManagerResult = DatabaseManagersFactory
                .CreateDatabaseManager(_logger, true, databaseServerConnectionData, apiClients, _httpClientFactory,
                    null, null, token).Preserve().Result;

            if (createDatabaseManagerResult.IsT1)
            {
                Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
                StShared.WriteErrorLine("dbManager could not created", true);
                return false;
            }

            Console.WriteLine("Try connect to server...");

            IDatabaseManager? dbManager = createDatabaseManagerResult.AsT0;

            Option<Err[]> dbmTestConnectionResult = dbManager.TestConnection(null, token).Result;
            if (dbmTestConnectionResult.IsSome)
            {
                Err.PrintErrorsOnConsole((Err[])dbmTestConnectionResult);
                return false;
            }

            if (dbManager is RemoteDatabaseManager)
            {
                return true;
            }

            //თუ დაკავშირება მოხერხდა, მაშინ დადგინდეს სერვერის მხარეს შემდეგი პარამეტრები:
            //ბექაპირების ფოლდერი, ბაზის აღდგენის ფოლდერი, ბაზის ლოგების ფაილის აღდგენის ფოლდერი.
            OneOf<DbServerInfo, Err[]> getDbServerInfoResult = dbManager.GetDatabaseServerInfo(token).Result;
            if (getDbServerInfoResult.IsT1)
            {
                Err.PrintErrorsOnConsole(getDbServerInfoResult.AsT1);
                return false;
            }

            DbServerInfo? dbServerInfo = getDbServerInfoResult.AsT0;

            Console.WriteLine($"Server Name is {dbServerInfo.ServerName}");
            Console.WriteLine(
                $"Server is {(dbServerInfo.AllowsCompression ? string.Empty : "NOT ")} Allows Compression");
            OneOf<bool, Err[]> isServerLocalResult = dbManager.IsServerLocal(token).Result;
            var notOrNot = isServerLocalResult.AsT0 ? string.Empty : "NOT ";
            Console.WriteLine(isServerLocalResult.IsT0
                ? $"Server is {notOrNot} local"
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
            _logger.LogError(e, "An exception occurred while validating the database server connection.");
        }

        return false;
    }

    protected override void CheckFieldsEnables(ItemData itemData, string? lastEditedFieldName = null)
    {
        var databaseServerConnection = (DatabaseServerConnectionData)itemData;

        var databaseServerProvider = databaseServerConnection.DatabaseServerProvider;
        (var toReturn, bool enablePassword, bool enableUser, bool enableSqlServerProps, bool enableWebAgentProps) =
            EnableDisableByDatabaseProvider(databaseServerProvider, databaseServerConnection);
        if (toReturn)
        {
            return;
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
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(databaseServerConnection.ServerAddress) &&
            !string.IsNullOrWhiteSpace(databaseServerConnection.ServerUser) &&
            !string.IsNullOrWhiteSpace(databaseServerConnection.ServerPass))
        {
            CheckValidation(itemData);
        }
    }

    private (bool, bool, bool, bool, bool) EnableDisableByDatabaseProvider(EDatabaseProvider databaseServerProvider,
        DatabaseServerConnectionData databaseServerConnection)
    {
        switch (databaseServerProvider)
        {
            case EDatabaseProvider.None:
                EnableAllFieldButOne(nameof(DatabaseServerConnectionData.DatabaseServerProvider), false);
                return (true, false, false, false, false);
            case EDatabaseProvider.SqlServer:
                var enablePassword = !databaseServerConnection.WindowsNtIntegratedSecurity;
                return (false, enablePassword, enablePassword, true, false);
            case EDatabaseProvider.SqLite:
            case EDatabaseProvider.OleDb:
                return (false, true, false, false, false);
            case EDatabaseProvider.WebAgent:
                return (false, false, false, false, true);
            default:
                throw new ArgumentOutOfRangeException(nameof(databaseServerProvider), databaseServerProvider,
                    "Unexpected database server provider value.");
        }
    }

    public override string GetStatusFor(string name)
    {
        var databaseServerConnection = (DatabaseServerConnectionData?)GetItemByName(name);
        return
            $"{databaseServerConnection?.DatabaseServerProvider.ToString()}: {databaseServerConnection?.ServerAddress}";
    }

    public override void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string itemName)
    {
        base.FillDetailsSubMenu(itemSubMenuSet, itemName);

        var parameters = (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        DatabaseServerConnectionData databaseServerConnectionDataByKey =
            parameters.DatabaseServerConnections[itemName];

        if (databaseServerConnectionDataByKey.DatabaseServerProvider == EDatabaseProvider.WebAgent)
        {
            return;
        }

        var getDbServerFoldersCliMenuCommand =
            new GetDbServerFoldersCliMenuCommand(_logger, _httpClientFactory, itemName, ParametersManager);
        itemSubMenuSet.AddMenuItem(getDbServerFoldersCliMenuCommand);
    }
}
