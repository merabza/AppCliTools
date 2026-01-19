using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParameters.FieldEditors;
using CliParametersDataEdit.Models;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Models;
using DatabaseTools.DbToolsFactory;
using DatabaseTools.SqlServerDbTools;
using LibDataInput;
using LibMenuInput;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibDatabaseParameters;

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
        try
        {
            string? currentDatabaseName = GetValue(recordForUpdate);
            string? serverAddress = GetValue<string>(recordForUpdate, _serverAddressPropertyName);
            bool windowsNtIntegratedSecurity =
                GetValue<bool>(recordForUpdate, _windowsNtIntegratedSecurityPropertyName);
            string? serverUser = GetValue<string>(recordForUpdate, _serverUserPropertyName);
            string? serverPass = GetValue<string>(recordForUpdate, _serverPassPropertyName);

            if (serverAddress is null || serverUser is null || serverPass is null)
            {
                throw new Exception("serverAddress is null or serverUser is null or serverPass is null");
            }

            var sqlSerConPar = new SqlServerConnectionParameters
            {
                ServerAddress = serverAddress,
                WindowsNtIntegratedSecurity = windowsNtIntegratedSecurity,
                ServerUser = serverUser,
                ServerPass = serverPass
            };

            DbConnectionStringBuilder dbConnectionStringBuilder =
                DbConnectionFactory.GetDbConnectionStringBuilder(sqlSerConPar) ??
                throw new Exception("dbConnectionStringBuilder is null");

            var dbKit = DbKitFactory.GetKit(EDatabaseProvider.SqlServer);
            DbClient dc = new SqlDbClient(_logger, (SqlConnectionStringBuilder)dbConnectionStringBuilder, dbKit, true);

            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            token.ThrowIfCancellationRequested();

            var getDatabaseInfosResult = dc.GetDatabaseInfos(token).Result;

            var databaseInfos = new List<DatabaseInfoModel>();
            if (getDatabaseInfosResult.IsT0)
            {
                databaseInfos = getDatabaseInfosResult.AsT0;
            }

            var databasesMenuSet = new CliMenuSet();
            databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand("New Database Name"));

            var keys = databaseInfos.Select(s => s.Name).ToList();
            foreach (string listItem in keys)
            {
                databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand(listItem));
            }

            int selectedId = MenuInputer.InputIdFromMenuList(FieldName, databasesMenuSet, currentDatabaseName);

            if (selectedId == 0)
            {
                string newDatabaseName = Inputer.InputTextRequired("New Database Name");

                SetValue(recordForUpdate, newDatabaseName);
                return;
            }

            int index = selectedId - 1;
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
            _logger.LogError(e, "Error in SqlServerDatabaseNameFieldEditor.UpdateField: {Message}", e.Message);
            throw new Exception("An error occurred while updating the SQL Server database name field.", e);
        }
    }
}
