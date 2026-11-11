using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;

namespace AppCliTools.CliParametersDataEdit.Tests;

internal static class CliMenuTestAccess
{
    //CliMenuSet keeps its items only in private state
    public static List<CliMenuItem> GetMenuItems(CliMenuSet menuSet)
    {
        return (List<CliMenuItem>)typeof(CliMenuSet)
            .GetProperty("MenuItems", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(menuSet)!;
    }

    //Run hides the result of RunBody when success and failure lead to the same menu action
    public static ValueTask<bool> RunBody(CliMenuCommand command)
    {
        return (ValueTask<bool>)typeof(CliMenuCommand)
            .GetMethod("RunBody", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(command, [CancellationToken.None])!;
    }
}
