﻿using System;
using System.Data.SqlClient;
using System.Linq;
using CliMenu;
using CliParameters.FieldEditors;
using CliParameters.MenuCommands;
using CliParametersDataEdit.Models;
using DbTools;
using DbToolsFabric;
using LibDataInput;
using LibMenuInput;
using Microsoft.Extensions.Logging;
using SqlServerDbTools;

namespace CliParametersDataEdit.FieldEditors;

public sealed class SqlServerDatabaseNameFieldEditor : FieldEditor<string>
{
    private readonly ILogger _logger;
    private readonly string _serverAddressPropertyName;
    private readonly string _serverPassPropertyName;
    private readonly string _serverUserPropertyName;
    private readonly string _windowsNtIntegratedSecurityPropertyName;

    public SqlServerDatabaseNameFieldEditor(ILogger logger, string propertyName, string serverAddressPropertyName,
        string windowsNtIntegratedSecurityPropertyName, string serverUserPropertyName,
        string serverPassPropertyName) :
        base(propertyName)
    {
        _logger = logger;
        _serverAddressPropertyName = serverAddressPropertyName;
        _windowsNtIntegratedSecurityPropertyName = windowsNtIntegratedSecurityPropertyName;
        _serverUserPropertyName = serverUserPropertyName;
        _serverPassPropertyName = serverPassPropertyName;
    }

    public override void UpdateField(string? recordName, object recordForUpdate) //, object currentRecord
    {
        var currentDatabaseName = GetValue(recordForUpdate);
        var serverAddress = GetValue<string>(recordForUpdate, _serverAddressPropertyName);
        var windowsNtIntegratedSecurity =
            GetValue<bool>(recordForUpdate, _windowsNtIntegratedSecurityPropertyName);
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

        var dbConnectionStringBuilder =
            DbConnectionFabric.GetDbConnectionStringBuilder(sqlSerConPar);

        if (dbConnectionStringBuilder is null)
            throw new Exception("dbConnectionStringBuilder is null");

        var dbKit = ManagerFactory.GetKit(EDataProvider.Sql);
        DbClient dc = new SqlDbClient(_logger, (SqlConnectionStringBuilder)dbConnectionStringBuilder,
            dbKit, true);
        var databaseInfos = dc.GetDatabaseInfos().Result;

        CliMenuSet databasesMenuSet = new();
        databasesMenuSet.AddMenuItem(new MenuCommandWithStatus(null), "New Database Name");

        var keys = databaseInfos.Select(s => s.Name).ToList();
        foreach (var listItem in keys)
            databasesMenuSet.AddMenuItem(new MenuCommandWithStatus(listItem), listItem);

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