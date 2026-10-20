using System.Collections.Generic;
using System.Reflection;
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
}
