using System;
using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliMenu.CliMenuCommands;
using Microsoft.Extensions.DependencyInjection;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliMenu;

public static class CliMenuSetFactory
{
    public static CliMenuSet? CreateMenuSet(string menuCaption, List<string> menuCommandNames,
        IServiceProvider serviceProvider, bool isMainMenu = false)
    {
        var mainMenuSet = new CliMenuSet(menuCaption);

        Dictionary<string, IMenuCommandFactoryStrategy>? menuCommandStrategies = serviceProvider
            .GetService<IEnumerable<IMenuCommandFactoryStrategy>>()?.ToDictionary(s => s.GetType().Name, s => s);

        if (menuCommandStrategies == null)
        {
            StShared.WriteErrorLine("No IMenuCommandFactoryStrategy implementations found", true);
            return null;
        }

        Dictionary<string, IMenuCommandListFactoryStrategy>? toolCommandListStrategies = serviceProvider
            .GetService<IEnumerable<IMenuCommandListFactoryStrategy>>()?.ToDictionary(s => s.GetType().Name, s => s);

        if (toolCommandListStrategies == null)
        {
            StShared.WriteErrorLine("No IMenuCommandListFactoryStrategy implementations found", true);
            return null;
        }

        foreach (string menuCommandName in menuCommandNames)
        {
            if (menuCommandStrategies.TryGetValue(menuCommandName, out IMenuCommandFactoryStrategy? value))
            {
                CliMenuCommand menuCommand = value.CreateMenuCommand();
                mainMenuSet.AddMenuItem(menuCommand);
            }
            else if (toolCommandListStrategies.TryGetValue(menuCommandName, out IMenuCommandListFactoryStrategy? list))
            {
                List<CliMenuCommand> menuCommandList = list.CreateMenuCommandsList();
                foreach (CliMenuCommand cliMenuCommand in menuCommandList)
                {
                    mainMenuSet.AddMenuItem(cliMenuCommand);
                }
            }
        }

        if (isMainMenu)
        {
            mainMenuSet.AddEscapeCommand(new ExitCliMenuCommand());
        }
        else
        {
            mainMenuSet.AddEscapeCommand();
        }

        return mainMenuSet;
    }
}
