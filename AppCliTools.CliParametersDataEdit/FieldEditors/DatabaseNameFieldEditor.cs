using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.LibDataInput;
using AppCliTools.LibMenuInput;
using DatabaseTools.DbTools.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using ToolsManagement.DatabasesManagement;

namespace AppCliTools.CliParametersDataEdit.FieldEditors;

public sealed class DatabaseNameFieldEditor : FieldEditor<string>
{
    private readonly bool _canUseNewDatabaseName;
    private readonly Func<string?, CancellationToken, Task<Result<IDatabaseManager>>> _createDatabaseManager;
    private readonly string _databaseConnectionNamePropertyName;
    private readonly Func<string, CliMenuSet, string?, int> _inputIdFromMenuList;
    private readonly Func<string, string> _inputTextRequired;

    public DatabaseNameFieldEditor(string appName, ILogger logger, IHttpClientFactory httpClientFactory,
        string propertyName, IParametersManager parametersManager, string databaseConnectionNamePropertyName,
        bool canUseNewDatabaseName) : this(propertyName, databaseConnectionNamePropertyName, canUseNewDatabaseName,
        (databaseServerConnectionName, cancellationToken) => CreateDatabaseManager(appName, logger, httpClientFactory,
            parametersManager, databaseServerConnectionName, cancellationToken), MenuInputer.InputIdFromMenuList,
        fieldName => Inputer.InputTextRequired(fieldName))
    {
    }

    //ბაზების მენეჯერის შექმნა და კონსოლიდან შეტანა პარამეტრებადაა გამოტანილი, რომ ტესტებმა ბაზების სია და პასუხები
    //თვითონ მიაწოდონ
    internal DatabaseNameFieldEditor(string propertyName, string databaseConnectionNamePropertyName,
        bool canUseNewDatabaseName,
        Func<string?, CancellationToken, Task<Result<IDatabaseManager>>> createDatabaseManager,
        Func<string, CliMenuSet, string?, int> inputIdFromMenuList, Func<string, string> inputTextRequired) : base(
        propertyName)
    {
        _databaseConnectionNamePropertyName = databaseConnectionNamePropertyName;
        _canUseNewDatabaseName = canUseNewDatabaseName;
        _createDatabaseManager = createDatabaseManager;
        _inputIdFromMenuList = inputIdFromMenuList;
        _inputTextRequired = inputTextRequired;
    }

    public override async ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string? currentDatabaseName = GetValue(recordForUpdate);
            string? databaseServerConnectionName =
                GetValue<string>(recordForUpdate, _databaseConnectionNamePropertyName);
            var databaseInfos = new List<DatabaseInfoModel>();

            Result<IDatabaseManager> createDatabaseManagerResult =
                await _createDatabaseManager(databaseServerConnectionName, cancellationToken);

            if (createDatabaseManagerResult.IsFailure)
            {
                createDatabaseManagerResult.Error.PrintErrorsOnConsole();
                //ზოგი პროვაიდერისთვის (მაგ. OleDb) მენეჯერი ჯერ არ არსებობს — ბაზების სია ვერ მიიღება,
                //მაგრამ სახელის ხელით შეყვანა მაინც შესაძლებელი უნდა იყოს
                if (!_canUseNewDatabaseName)
                {
                    return;
                }
            }
            else
            {
                Result<List<DatabaseInfoModel>> getDatabaseNamesResult =
                    await createDatabaseManagerResult.Value.GetDatabaseNames(cancellationToken);
                if (getDatabaseNamesResult.IsSuccess)
                {
                    databaseInfos = getDatabaseNamesResult.Value;
                }
                else
                {
                    getDatabaseNamesResult.Error.PrintErrorsOnConsole();
                }
            }

            //ელემენტის id მისი ადგილია სიაში (New Database Name, თუ არის, პირველია), ისევე, როგორც კლავიშით
            //არჩევისას. Enter მიმდინარე სახელს ტოვებს: სია მისი ელემენტის id-ს აბრუნებს, id-ის გარეშე დამატებულ
            //ელემენტს კი -1 აქვს, რაც არასწორ არჩევანად ჩაითვლებოდა
            int id = 0;
            var databasesMenuSet = new CliMenuSet();
            if (_canUseNewDatabaseName)
            {
                databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand("New Database Name"), id++);
            }

            List<string> keys = [.. databaseInfos.Select(s => s.Name)];
            foreach (string listItem in keys)
            {
                databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand(listItem), id++);
            }

            int selectedId = _inputIdFromMenuList(FieldName, databasesMenuSet, currentDatabaseName);

            if (_canUseNewDatabaseName && selectedId == 0)
            {
                string newDatabaseName = _inputTextRequired("New Database Name"); // nameInput.Text;
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

    private static Task<Result<IDatabaseManager>> CreateDatabaseManager(string appName, ILogger logger,
        IHttpClientFactory httpClientFactory, IParametersManager parametersManager,
        string? databaseServerConnectionName, CancellationToken cancellationToken)
    {
        var dscParameters = (IParametersWithDatabaseServerConnections)parametersManager.Parameters;
        var databaseServerConnections = new DatabaseServerConnections(dscParameters.DatabaseServerConnections);
        var acParameters = (IParametersWithApiClients)parametersManager.Parameters;
        var apiClients = new ApiClients(acParameters.ApiClients);
        return DatabaseManagersFactory.CreateDatabaseManager(appName, logger, true, databaseServerConnectionName,
            databaseServerConnections, apiClients, httpClientFactory, null, null, cancellationToken);
    }
}
