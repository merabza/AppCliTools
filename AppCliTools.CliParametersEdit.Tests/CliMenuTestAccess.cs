using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersEdit.Tests;

internal static class CliMenuTestAccess
{
    //სიის მენიუს New ბრძანება იმ cruder-ს ინახავს, რომელმაც მენიუ ააწყო. მენიუს ჩანაწერები და ბრძანების
    //cruder დახურულ ველებშია
    public static Cruder GetListMenuCruder(CliMenuSet listMenu)
    {
        NewItemCliMenuCommand newItemCommand =
            GetMenuItems(listMenu).Select(x => x.CliMenuCommand).OfType<NewItemCliMenuCommand>().Single();
        return (Cruder)typeof(NewItemCliMenuCommand)
            .GetField("_cruder", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(newItemCommand)!;
    }

    //CliMenuSet keeps its items only in private state
    public static List<CliMenuItem> GetMenuItems(CliMenuSet menuSet)
    {
        return (List<CliMenuItem>)typeof(CliMenuSet)
            .GetProperty("MenuItems", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(menuSet)!;
    }

    //Cruder ველების რედაქტორებს protected სიაში ინახავს
    public static List<FieldEditor> GetFieldEditors(Cruder cruder)
    {
        return (List<FieldEditor>)typeof(Cruder)
            .GetField("FieldEditors", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(cruder)!;
    }

    //the record methods are protected overrides; the menu commands that call them ask on the console first
    public static Dictionary<string, ItemData> GetCrudersDictionary(Cruder cruder)
    {
        return (Dictionary<string, ItemData>)GetProtectedMethod(cruder, "GetCrudersDictionary").Invoke(cruder, [])!;
    }

    public static ItemData InvokeCreateNewItem(Cruder cruder)
    {
        return (ItemData)GetProtectedMethod(cruder, "CreateNewItem").Invoke(cruder, [null, null])!;
    }

    public static async Task InvokeRemoveRecordWithKey(Cruder cruder, string recordKey)
    {
        await (ValueTask)GetProtectedMethod(cruder, "RemoveRecordWithKey")
            .Invoke(cruder, [recordKey, CancellationToken.None])!;
    }

    public static async Task InvokeAddRecordWithKey(Cruder cruder, string recordKey, ItemData newRecord)
    {
        await (ValueTask)GetProtectedMethod(cruder, "AddRecordWithKey")
            .Invoke(cruder, [recordKey, newRecord, CancellationToken.None])!;
    }

    private static MethodInfo GetProtectedMethod(Cruder cruder, string methodName)
    {
        return cruder.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;
    }
}
