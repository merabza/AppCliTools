using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliMenu;

public static class CliMenuSetFactory
{
    public static CliMenuSet CreateMenuSet(string menuCaption, List<string> menuCommandNames,
        ServiceProvider serviceProvider, ParametersManager parametersManager)
    {
        var mainMenuSet = new CliMenuSet(menuCaption);

        foreach (CliMenuCommand? menuCommand in menuCommandNames.Select(menuCommandName =>
                     MenuCommandFactory.CreateMenuCommand(menuCommandName, serviceProvider, parametersManager)))
        {
            mainMenuSet.AddMenuItem(menuCommand!);
        }

        return mainMenuSet;
    }
}
