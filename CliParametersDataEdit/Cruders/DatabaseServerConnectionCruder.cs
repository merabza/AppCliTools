using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CliMenu;
using CliParameters;
using CliParameters.FieldEditors;
using CliParametersDataEdit.CliMenuCommands;
using CliParametersDataEdit.FieldEditors;
using DbTools;
using DbToolsFabric;
using LibDatabaseParameters;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace CliParametersDataEdit.Cruders;

public sealed class DatabaseServerConnectionCruder : ParCruder
{
    private readonly ILogger _logger;

    public DatabaseServerConnectionCruder(IParametersManager parametersManager, ILogger logger) : base(
        parametersManager, "Database Server Connection", "Database Server Connections")
    {
        _logger = logger;
        FieldEditors.Add(new EnumFieldEditor<EDatabaseServerProvider>(
            nameof(DatabaseServerConnectionData.DatabaseServerProvider), EDatabaseServerProvider.SqlServer));
        FieldEditors.Add(new BoolFieldEditor(nameof(DatabaseServerConnectionData.WindowsNtIntegratedSecurity), false));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.ServerAddress)));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.ServerUser)));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.ServerPass), null,
            ParametersEditor.PasswordChar));
        FieldEditors.Add(new BoolFieldEditor(nameof(DatabaseServerConnectionData.TrustServerCertificate), true));
        FieldEditors.Add(new BoolFieldEditor(nameof(DatabaseServerConnectionData.Encrypt), false));
        FieldEditors.Add(new IntFieldEditor(nameof(DatabaseServerConnectionData.ConnectionTimeOut), 15));
        FieldEditors.Add(new DatabaseBackupParametersFieldEditor(logger,
            nameof(DatabaseServerConnectionData.FullDbBackupParameters), parametersManager));
        FieldEditors.Add(new DatabaseFoldersSetFieldEditor(parametersManager,
            nameof(DatabaseServerConnectionData.DatabaseFoldersSets)));
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        var parameters = (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        return parameters.DatabaseServerConnections.ToDictionary(p => p.Key, ItemData (p) => p.Value);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        var parameters = (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        var databaseServerConnections = parameters.DatabaseServerConnections;
        return databaseServerConnections.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        var newDatabaseServerConnection = (DatabaseServerConnectionData)newRecord;
        var parameters = (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        parameters.DatabaseServerConnections[recordKey] = newDatabaseServerConnection;
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        var newDatabaseServerConnection = (DatabaseServerConnectionData)newRecord;
        var parameters = (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        parameters.DatabaseServerConnections.Add(recordKey, newDatabaseServerConnection);
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        var parameters = (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        var databaseServerConnections = parameters.DatabaseServerConnections;
        databaseServerConnections.Remove(recordKey);
    }

    public override bool CheckValidation(ItemData item)
    {
        try
        {
            if (item is not DatabaseServerConnectionData databaseServerConnectionData)
                return false;
            switch (databaseServerConnectionData.DatabaseServerProvider)
            {
                case EDatabaseServerProvider.SqlServer:
                    Console.WriteLine($"Try connect to server {databaseServerConnectionData.ServerAddress}...");

                    //მოისინჯოს ბაზასთან დაკავშირება.
                    //თუ დაკავშირება ვერ მოხერხდა, გამოვიდეს ამის შესახებ შეტყობინება და შევთავაზოთ მონაცემების შეყვანის გაგრძელება, ან გაჩერება
                    //აქ გამოიყენება ბაზასთან პირდაპირ დაკავშირება ვებაგენტის გარეშე,
                    //რადგან სწორედ ასეთი ტიპის კავშირების რედაქტორია ეს.

                    if (string.IsNullOrWhiteSpace(databaseServerConnectionData.ServerAddress))
                    {
                        StShared.WriteErrorLine("ServerAddress is not specified", true);
                        return false;
                    }

                    var dbAuthSettings = DbAuthSettingsCreator.Create(
                        databaseServerConnectionData.WindowsNtIntegratedSecurity,
                        databaseServerConnectionData.ServerUser, databaseServerConnectionData.ServerPass);

                    if (dbAuthSettings is null)
                    {
                        StShared.WriteErrorLine("Authentication parameters is not valid", true);
                        return false;
                    }

                    var dc = DbClientFabric.GetDbClient(_logger, true, EDataProvider.Sql,
                        databaseServerConnectionData.ServerAddress, dbAuthSettings,
                        databaseServerConnectionData.TrustServerCertificate, ProgramAttributes.Instance.AppName);

                    if (dc is null)
                    {
                        StShared.WriteErrorLine("Database Client is not created", true);
                        return false;
                    }

                    var testConnectionResult = dc.TestConnection(false, CancellationToken.None).Result;
                    if (testConnectionResult.IsSome)
                    {
                        Err.PrintErrorsOnConsole((Err[])testConnectionResult);
                        return false;
                    }

                    //თუ დაკავშირება მოხერხდა, მაშინ დადგინდეს სერვერის მხარეს შემდეგი პარამეტრები:
                    //ბექაპირების ფოლდერი, ბაზის აღდგენის ფოლდერი, ბაზის ლოგების ფაილის აღდგენის ფოლდერი.
                    var getDbServerInfoResult = dc.GetDbServerInfo(CancellationToken.None).Result;
                    if (getDbServerInfoResult.IsT1)
                    {
                        Err.PrintErrorsOnConsole(getDbServerInfoResult.AsT1);
                        return false;
                    }

                    var dbServerInfo = getDbServerInfoResult.AsT0;

                    Console.WriteLine($"Server Name is {dbServerInfo.ServerName}");
                    Console.WriteLine(
                        $"Server is {(dbServerInfo.AllowsCompression ? string.Empty : "NOT ")} Allows Compression");
                    var isServerLocalResult = dc.IsServerLocal(CancellationToken.None).Result;
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }

    protected override void CheckFieldsEnables(ItemData itemData, string? lastEditedFieldName = null)
    {
        var databaseServerConnection = (DatabaseServerConnectionData)itemData;
        var enable = databaseServerConnection is
            { DatabaseServerProvider: EDatabaseServerProvider.SqlServer, WindowsNtIntegratedSecurity: false };
        EnableFieldByName(nameof(DatabaseServerConnectionData.ServerUser), enable);
        EnableFieldByName(nameof(DatabaseServerConnectionData.ServerPass), enable);

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

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new DatabaseServerConnectionData();
    }

    public override void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string recordKey)
    {
        base.FillDetailsSubMenu(itemSubMenuSet, recordKey);

        GetDbServerFoldersCliMenuCommand getDbServerFoldersCliMenuCommand = new(_logger, recordKey, ParametersManager);
        itemSubMenuSet.AddMenuItem(getDbServerFoldersCliMenuCommand);

        //ამ ვარიანტმა არ იმუშავა
        //PutDbServerFoldersCliMenuCommand putDbServerFoldersCliMenuCommand = new(_logger, recordKey, ParametersManager);
        //itemSubMenuSet.AddMenuItem(putDbServerFoldersCliMenuCommand, "Put Database Server Folders from parameters to server");
    }
}