using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliMenu;

public static class MenuCommandFactory
{
    public static CliMenuCommand? CreateMenuCommand(string menuCommandName, ServiceProvider serviceProvider,
        IParametersManager parametersManager)
    {
        Dictionary<string, IMenuCommandFactoryStrategy>? toolCommandStrategies = serviceProvider
            .GetService<IEnumerable<IMenuCommandFactoryStrategy>>()?.ToDictionary(s => s.MenuCommandName, s => s);

        if (toolCommandStrategies == null)
        {
            StShared.WriteErrorLine("No IMenuCommandFactoryStrategy implementations found", true);
            return null;
        }

        if (toolCommandStrategies.TryGetValue(menuCommandName, out IMenuCommandFactoryStrategy? strategy))
        {
            return strategy.CreateMenuCommand(parametersManager);
        }

        StShared.WriteErrorLine($"No strategy found for menu command {menuCommandName}", true);
        return null;
    }
}
