using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersDataEdit.Models;
using AppCliTools.LibDataInput;
using AppCliTools.LibMenuInput;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Models;
using DatabaseTools.DbToolsFactory;
using DatabaseTools.SqlServerDbTools;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace AppCliTools.CliParametersDataEdit.FieldEditors;

public sealed class SqlServerDatabaseNameFieldEditor : FieldEditor<string>
{
    //SqlClient-ის ნაგულისხმევი კავშირის დრო წამებში
    private const int DefaultConnectionTimeOut = 15;

    private readonly string _connectionTimeOutPropertyName;
    private readonly string _encryptPropertyName;

    private readonly Func<SqlServerConnectionParameters, CancellationToken, Task<Result<List<DatabaseInfoModel>>>>
        _getDatabaseInfos;

    private readonly Func<string, CliMenuSet, string?, int> _inputIdFromMenuList;
    private readonly Func<string, string> _inputTextRequired;
    private readonly ILogger _logger;
    private readonly string _serverAddressPropertyName;
    private readonly string _serverPassPropertyName;
    private readonly string _serverUserPropertyName;
    private readonly string _trustServerCertificatePropertyName;
    private readonly string _windowsNtIntegratedSecurityPropertyName;

    public SqlServerDatabaseNameFieldEditor(ILogger logger, string propertyName, string serverAddressPropertyName,
        string windowsNtIntegratedSecurityPropertyName, string serverUserPropertyName, string serverPassPropertyName,
        string connectionTimeOutPropertyName, string encryptPropertyName, string trustServerCertificatePropertyName,
        bool enterFieldDataOnCreate = false) : this(logger, propertyName, serverAddressPropertyName,
        windowsNtIntegratedSecurityPropertyName, serverUserPropertyName, serverPassPropertyName,
        connectionTimeOutPropertyName, encryptPropertyName, trustServerCertificatePropertyName, enterFieldDataOnCreate,
        (sqlSerConPar, cancellationToken) => GetDatabaseInfos(logger, sqlSerConPar, cancellationToken),
        MenuInputer.InputIdFromMenuList, fieldName => Inputer.InputTextRequired(fieldName))
    {
    }

    //ბაზების სიის მიღება და კონსოლიდან შეტანა პარამეტრებადაა გამოტანილი, რომ ტესტებმა ბაზების სია და პასუხები
    //თვითონ მიაწოდონ
    internal SqlServerDatabaseNameFieldEditor(ILogger logger, string propertyName, string serverAddressPropertyName,
        string windowsNtIntegratedSecurityPropertyName, string serverUserPropertyName, string serverPassPropertyName,
        string connectionTimeOutPropertyName, string encryptPropertyName, string trustServerCertificatePropertyName,
        bool enterFieldDataOnCreate,
        Func<SqlServerConnectionParameters, CancellationToken, Task<Result<List<DatabaseInfoModel>>>> getDatabaseInfos,
        Func<string, CliMenuSet, string?, int> inputIdFromMenuList, Func<string, string> inputTextRequired) : base(
        propertyName, enterFieldDataOnCreate)
    {
        _logger = logger;
        _serverAddressPropertyName = serverAddressPropertyName;
        _windowsNtIntegratedSecurityPropertyName = windowsNtIntegratedSecurityPropertyName;
        _serverUserPropertyName = serverUserPropertyName;
        _serverPassPropertyName = serverPassPropertyName;
        _connectionTimeOutPropertyName = connectionTimeOutPropertyName;
        _encryptPropertyName = encryptPropertyName;
        _trustServerCertificatePropertyName = trustServerCertificatePropertyName;
        _getDatabaseInfos = getDatabaseInfos;
        _inputIdFromMenuList = inputIdFromMenuList;
        _inputTextRequired = inputTextRequired;
    }

    public override async ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string? currentDatabaseName = GetValue(recordForUpdate);
            string? serverAddress = GetValue<string>(recordForUpdate, _serverAddressPropertyName);
            bool windowsNtIntegratedSecurity =
                GetValue<bool>(recordForUpdate, _windowsNtIntegratedSecurityPropertyName);
            string? serverUser = GetValue<string>(recordForUpdate, _serverUserPropertyName);
            string? serverPass = GetValue<string>(recordForUpdate, _serverPassPropertyName);
            int connectionTimeOut = GetValue<int>(recordForUpdate, _connectionTimeOutPropertyName);
            bool encrypt = GetValue<bool>(recordForUpdate, _encryptPropertyName);
            bool trustServerCertificate = GetValue<bool>(recordForUpdate, _trustServerCertificatePropertyName);

            if (serverAddress is null)
            {
                throw new Exception("serverAddress is null");
            }

            //Windows-ის ავთენტიფიკაციისას მომხმარებელი და პაროლი არ გამოიყენება
            if (!windowsNtIntegratedSecurity && (serverUser is null || serverPass is null))
            {
                throw new Exception("serverUser or serverPass is null");
            }

            //სია ჩანაწერის კავშირის პარამეტრებით მიიღება. კავშირის დრო 0 SqlClient-ში ლოდინს ლიმიტის გარეშე ნიშნავს
            //და მიუწვდომელი სერვერი მენიუს გაჭედავდა, ამიტომ მაშინ ნაგულისხმევი დრო გამოიყენება
            var sqlSerConPar = new SqlServerConnectionParameters
            {
                ServerAddress = serverAddress,
                WindowsNtIntegratedSecurity = windowsNtIntegratedSecurity,
                ServerUser = serverUser,
                ServerPass = serverPass,
                ConnectionTimeOut = connectionTimeOut > 0 ? connectionTimeOut : DefaultConnectionTimeOut,
                Encrypt = encrypt,
                TrustServerCertificate = trustServerCertificate
            };

            Result<List<DatabaseInfoModel>> getDatabaseInfosResult =
                await _getDatabaseInfos(sqlSerConPar, cancellationToken);

            var databaseInfos = new List<DatabaseInfoModel>();
            if (getDatabaseInfosResult.IsSuccess)
            {
                databaseInfos = getDatabaseInfosResult.Value;
            }

            //ელემენტის id მისი ადგილია სიაში (New Database Name პირველია), ისევე, როგორც კლავიშით არჩევისას. Enter
            //მიმდინარე სახელს ტოვებს: სია მისი ელემენტის id-ს აბრუნებს, id-ის გარეშე დამატებულ ელემენტს კი -1 აქვს,
            //რაც არასწორ არჩევანად ჩაითვლებოდა
            int id = 0;
            var databasesMenuSet = new CliMenuSet();
            databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand("New Database Name"), id++);

            List<string> keys = [.. databaseInfos.Select(s => s.Name)];
            foreach (string listItem in keys)
            {
                databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand(listItem), id++);
            }

            int selectedId = _inputIdFromMenuList(FieldName, databasesMenuSet, currentDatabaseName);

            if (selectedId == 0)
            {
                string newDatabaseName = _inputTextRequired("New Database Name");

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
            _logger.LogError(e, "ErrorOmd in SqlServerDatabaseNameFieldEditor.UpdateField: {Message}", e.Message);
            throw new Exception("An error occurred while updating the SQL Server database name field.", e);
        }
    }

    private static Task<Result<List<DatabaseInfoModel>>> GetDatabaseInfos(ILogger logger,
        SqlServerConnectionParameters sqlSerConPar, CancellationToken cancellationToken)
    {
        DbConnectionStringBuilder dbConnectionStringBuilder =
            DbConnectionFactory.GetDbConnectionStringBuilder(sqlSerConPar) ??
            throw new Exception("dbConnectionStringBuilder is null");

        DbKit dbKit = DbKitFactory.GetKit(EDatabaseProvider.SqlServer);
        DbClient dc = new SqlDbClient(logger, (SqlConnectionStringBuilder)dbConnectionStringBuilder, dbKit, true);

        return dc.GetDatabaseInfos(cancellationToken);
    }
}
