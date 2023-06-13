using System;
using System.Collections.Generic;
using System.Linq;
using CliParameters;
using CliParameters.FieldEditors;
using DbTools;
using DbToolsFabric;
using LibDatabaseParameters;
using LibParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace CliParametersDataEdit.Cruders;

public sealed class DatabaseServerConnectionCruder : ParCruder
{
    private readonly ILogger _logger;

    public DatabaseServerConnectionCruder(IParametersManager parametersManager, ILogger logger) : base(
        parametersManager, "Database Server Connection", "Database Server Connections")
    {
        _logger = logger;
        FieldEditors.Add(new EnumFieldEditor<EDataProvider>(nameof(DatabaseServerConnectionData.DataProvider),
            EDataProvider.Sql));
        FieldEditors.Add(new BoolFieldEditor(nameof(DatabaseServerConnectionData.WindowsNtIntegratedSecurity), false));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.ServerAddress)));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.ServerUser)));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.ServerPass), default, '*'));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.BackupFolderName)));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.DataFolderName)));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseServerConnectionData.DataLogFolderName)));
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        var parameters =
            (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        return parameters.DatabaseServerConnections.ToDictionary(p => p.Key, p => (ItemData)p.Value);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        var parameters =
            (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        var
            databaseServerConnections = parameters.DatabaseServerConnections;
        return databaseServerConnections.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordName, ItemData newRecord)
    {
        var newDatabaseServerConnection = (DatabaseServerConnectionData)newRecord;
        var parameters =
            (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        parameters.DatabaseServerConnections[recordName] = newDatabaseServerConnection;
    }

    protected override void AddRecordWithKey(string recordName, ItemData newRecord)
    {
        var newDatabaseServerConnection = (DatabaseServerConnectionData)newRecord;
        var parameters =
            (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        parameters.DatabaseServerConnections.Add(recordName, newDatabaseServerConnection);
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        var parameters =
            (IParametersWithDatabaseServerConnections)ParametersManager.Parameters;
        var
            databaseServerConnections = parameters.DatabaseServerConnections;
        databaseServerConnections.Remove(recordKey);
    }

    public override bool CheckValidation(ItemData item)
    {
        try
        {
            if (item is not DatabaseServerConnectionData databaseServerConnectionData)
                return false;
            switch (databaseServerConnectionData.DataProvider)
            {
                case EDataProvider.Sql:
                    Console.WriteLine($"Try connect to server {databaseServerConnectionData.ServerAddress}...");

                    //მოისინჯოს ბაზასთან დაკავშირება.
                    //თუ დაკავშირება ვერ მოხერხდა, გამოვიდეს ამის შესახებ შეტყობინება და შევთავაზოთ მონაცემების შეყვანის გაგრძელება, ან გაჩერება
                    //აქ გამოიყენება ბაზასთან პირდაპირ დაკავშირება ვებაგენტის გარეშე,
                    //რადგან სწორედ ასეთი ტიპის კავშირების რედაქტორია ეს.

                    if (string.IsNullOrWhiteSpace(databaseServerConnectionData.ServerAddress))
                        return false;

                    var dbAuthSettings = DbAuthSettingsCreator.Create(
                        databaseServerConnectionData.WindowsNtIntegratedSecurity,
                        databaseServerConnectionData.ServerUser, databaseServerConnectionData.ServerPass);

                    if (dbAuthSettings is null)
                        return false;

                    var dc = DbClientFabric.GetDbClient(_logger, true, databaseServerConnectionData.DataProvider,
                        databaseServerConnectionData.ServerAddress, dbAuthSettings,
                        ProgramAttributes.Instance.GetAttribute<string>("AppName"));

                    if (dc is null)
                        return false;

                    if (!dc.TestConnection(false))
                        return false;

                    //თუ დაკავშირება მოხერხდა, მაშინ დადგინდეს სერვერის მხარეს შემდეგი პარამეტრები:
                    //ბექაპირების ფოლდერი, ბაზის აღდგენის ფოლდერი, ბაზის ლოგების ფაილის აღდგენის ფოლდერი.
                    var dbServerInfo = dc.GetDbServerInfo().Result;

                    Console.WriteLine($"Server Name is {dbServerInfo.ServerName}");
                    Console.WriteLine(
                        $"Server is {(dbServerInfo.AllowsCompression ? "" : "NOT ")} Allows Compression");
                    Console.WriteLine($"Server is {(dc.IsServerLocal() ? "" : "NOT ")} local");

                    Console.WriteLine($"Server Product Version is {dbServerInfo.ServerProductVersion}");
                    Console.WriteLine($"Server Instance Name is {dbServerInfo.ServerInstanceName}");
                    Console.WriteLine($"Backup Directory is {dbServerInfo.BackupDirectory}");
                    Console.WriteLine($"Default Data Directory is {dbServerInfo.DefaultDataDirectory}");
                    Console.WriteLine($"Default Log Directory is {dbServerInfo.DefaultLogDirectory}");

                    if (string.IsNullOrWhiteSpace(databaseServerConnectionData.BackupFolderName))
                        databaseServerConnectionData.BackupFolderName = dbServerInfo.BackupDirectory;

                    if (string.IsNullOrWhiteSpace(databaseServerConnectionData.DataFolderName))
                        databaseServerConnectionData.DataFolderName = dbServerInfo.DefaultDataDirectory;

                    if (string.IsNullOrWhiteSpace(databaseServerConnectionData.DataLogFolderName))
                        databaseServerConnectionData.DataLogFolderName = dbServerInfo.DefaultLogDirectory;

                    return true;
                case EDataProvider.SqLite:
                    return
                        false; //აქ ფაილის შემოწმება არის გასაკეთებელი. ჭეშმარიტი დაბრუნდეს, თუ ფაილი არსებობს და იხსნება
            }

            return false;
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
        var enable = databaseServerConnection.DataProvider == EDataProvider.Sql &&
                     !databaseServerConnection.WindowsNtIntegratedSecurity;
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
        return $"{databaseServerConnection?.DataProvider.ToString()}: {databaseServerConnection?.ServerAddress}";
    }

    protected override ItemData CreateNewItem(string recordName, ItemData? defaultItemData)
    {
        return new DatabaseServerConnectionData();
    }
}