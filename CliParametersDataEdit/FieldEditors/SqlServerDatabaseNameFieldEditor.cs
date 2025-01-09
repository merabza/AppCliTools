using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParameters.FieldEditors;
using CliParametersDataEdit.Models;
using DbTools;
using DbTools.Models;
using DbToolsFabric;
using LibDataInput;
using LibMenuInput;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SqlServerDbTools;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParametersDataEdit.FieldEditors;

public sealed class SqlServerDatabaseNameFieldEditor : FieldEditor<string>
{
    private readonly ILogger _logger;
    private readonly string _serverAddressPropertyName;
    private readonly string _serverPassPropertyName;
    private readonly string _serverUserPropertyName;
    private readonly string _windowsNtIntegratedSecurityPropertyName;

    public SqlServerDatabaseNameFieldEditor(ILogger logger, string propertyName, string serverAddressPropertyName,
        string windowsNtIntegratedSecurityPropertyName, string serverUserPropertyName, string serverPassPropertyName,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate)
    {
        _logger = logger;
        _serverAddressPropertyName = serverAddressPropertyName;
        _windowsNtIntegratedSecurityPropertyName = windowsNtIntegratedSecurityPropertyName;
        _serverUserPropertyName = serverUserPropertyName;
        _serverPassPropertyName = serverPassPropertyName;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        var currentDatabaseName = GetValue(recordForUpdate);
        var serverAddress = GetValue<string>(recordForUpdate, _serverAddressPropertyName);
        var windowsNtIntegratedSecurity = GetValue<bool>(recordForUpdate, _windowsNtIntegratedSecurityPropertyName);
        var serverUser = GetValue<string>(recordForUpdate, _serverUserPropertyName);
        var serverPass = GetValue<string>(recordForUpdate, _serverPassPropertyName);

        if (serverAddress is null || serverUser is null || serverPass is null)
            throw new Exception("serverAddress is null or serverUser is null or serverPass is null");

        SqlServerConnectionParameters sqlSerConPar = new()
        {
            ServerAddress = serverAddress,
            WindowsNtIntegratedSecurity = windowsNtIntegratedSecurity,
            ServerUser = serverUser,
            ServerPass = serverPass
        };

        var dbConnectionStringBuilder = DbConnectionFabric.GetDbConnectionStringBuilder(sqlSerConPar) ??
                                        throw new Exception("dbConnectionStringBuilder is null");

        var dbKit = ManagerFactory.GetKit(EDataProvider.Sql);
        DbClient dc = new SqlDbClient(_logger, (SqlConnectionStringBuilder)dbConnectionStringBuilder, dbKit, true);
        var getDatabaseInfosResult = dc.GetDatabaseInfos(CancellationToken.None).Result;

        var databaseInfos = new List<DatabaseInfoModel>();
        if (getDatabaseInfosResult.IsT0)
            databaseInfos = getDatabaseInfosResult.AsT0;

        CliMenuSet databasesMenuSet = new();
        databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand("New Database Name"));


        var keys = databaseInfos.Select(s => s.Name).ToList();
        foreach (var listItem in keys)
            databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand(listItem));

        var selectedId = MenuInputer.InputIdFromMenuList(FieldName, databasesMenuSet, currentDatabaseName);

        if (selectedId == 0)
        {
            var newDatabaseName = Inputer.InputTextRequired("New Database Name");

            SetValue(recordForUpdate, newDatabaseName);
            return;
        }

        var index = selectedId - 1;
        if (index < 0 || index >= keys.Count)
            throw new DataInputException("Selected invalid ID. ");

        SetValue(recordForUpdate, keys[index]);
    }
}