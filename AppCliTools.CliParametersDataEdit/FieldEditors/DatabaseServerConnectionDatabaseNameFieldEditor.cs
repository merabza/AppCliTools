using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.LibDataInput;
using AppCliTools.LibMenuInput;
using DatabaseTools.DbTools.Models;
using ParametersManagement.LibDatabaseParameters;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using ToolsManagement.DatabasesManagement;

namespace AppCliTools.CliParametersDataEdit.FieldEditors;

//Database Server Connection-ის არასავალდებულო ბაზის სახელი. სიაშია (None) — ბაზის გარეშე, New Database Name — სახელის
//ხელით შეყვანა და ჩანაწერის სერვერზე არსებული ბაზები. ბაზების მენეჯერი ჩანაწერის ველებით იქმნება, რადგან ახალი ან
//შეცვლილი კავშირი პარამეტრებში ჯერ შენახული არ არის
public sealed class DatabaseServerConnectionDatabaseNameFieldEditor : FieldEditor<string>
{
    private const string NewDatabaseNameItemName = "New Database Name";

    private readonly Func<DatabaseServerConnectionData, CancellationToken, ValueTask<Result<IDatabaseManager>>>
        _createDatabaseManager;

    private readonly Func<string, CliMenuSet, string?, int> _inputIdFromMenuList;
    private readonly Func<string, string> _inputTextRequired;

    public DatabaseServerConnectionDatabaseNameFieldEditor(string propertyName,
        Func<DatabaseServerConnectionData, CancellationToken, ValueTask<Result<IDatabaseManager>>>
            createDatabaseManager) : this(propertyName, createDatabaseManager, MenuInputer.InputIdFromMenuList,
        fieldName => Inputer.InputTextRequired(fieldName))
    {
    }

    //კონსოლიდან შეტანა პარამეტრებადაა გამოტანილი, რომ ტესტებმა პასუხები თვითონ მიაწოდონ
    internal DatabaseServerConnectionDatabaseNameFieldEditor(string propertyName,
        Func<DatabaseServerConnectionData, CancellationToken, ValueTask<Result<IDatabaseManager>>>
            createDatabaseManager, Func<string, CliMenuSet, string?, int> inputIdFromMenuList,
        Func<string, string> inputTextRequired) : base(propertyName)
    {
        _createDatabaseManager = createDatabaseManager;
        _inputIdFromMenuList = inputIdFromMenuList;
        _inputTextRequired = inputTextRequired;
    }

    public override async ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        List<string> databaseNames =
            await GetDatabaseNames((DatabaseServerConnectionData)recordForUpdate, cancellationToken);

        //(None)-ს id -1-ია. დანარჩენი ელემენტების id მათი ადგილია სიაში (New Database Name პირველია), რომ Enter-მა
        //მიმდინარე სახელი დატოვოს: id-ის გარეშე დამატებულ ელემენტს -1 ექნებოდა, რაც (None)-ად ჩაითვლებოდა
        var databasesMenuSet = new CliMenuSet();
        databasesMenuSet.AddMenuItem("-", new CliMenuCommand("(None)"));
        int id = 0;
        databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand(NewDatabaseNameItemName), id++);
        foreach (string databaseName in databaseNames)
        {
            databasesMenuSet.AddMenuItem(new MenuCommandWithStatusCliMenuCommand(databaseName), id++);
        }

        int selectedId = _inputIdFromMenuList(FieldName, databasesMenuSet, GetValue(recordForUpdate));

        switch (selectedId)
        {
            case -1:
                SetValue(recordForUpdate, null);
                return;
            case 0:
                SetValue(recordForUpdate, _inputTextRequired(NewDatabaseNameItemName));
                return;
        }

        if (selectedId < 0 || selectedId > databaseNames.Count)
        {
            throw new DataInputException("Selected invalid ID. ");
        }

        SetValue(recordForUpdate, databaseNames[selectedId - 1]);
    }

    //სერვერი თუ არ პასუხობს ან ბაზების სიას ვერ იძლევა, (None) და New Database Name მაინც შეიძლება აირჩეს
    private async ValueTask<List<string>> GetDatabaseNames(DatabaseServerConnectionData databaseServerConnection,
        CancellationToken cancellationToken)
    {
        Result<IDatabaseManager> createDatabaseManagerResult =
            await _createDatabaseManager(databaseServerConnection, cancellationToken);
        if (createDatabaseManagerResult.IsFailure)
        {
            createDatabaseManagerResult.Error.PrintErrorsOnConsole();
            return [];
        }

        Result<List<DatabaseInfoModel>> getDatabaseNamesResult =
            await createDatabaseManagerResult.Value.GetDatabaseNames(cancellationToken);
        if (getDatabaseNamesResult.IsFailure)
        {
            getDatabaseNamesResult.Error.PrintErrorsOnConsole();
            return [];
        }

        return [.. getDatabaseNamesResult.Value.Select(x => x.Name)];
    }
}
