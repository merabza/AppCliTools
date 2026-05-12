using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliMenu;
using AppCliTools.CliTools.Services.RecentCommands;

namespace AppCliTools.CliTools.Menu.RecentCommandsList;

// ReSharper disable once UnusedType.Global
public class RecentCommandsListFactoryStrategy(IRecentCommandsService recentCommandsService)
    : IMenuCommandListFactoryStrategy
{
    public List<CliMenuCommand> CreateMenuCommandsList()
    {
        return recentCommandsService.GetRecentCommands().Cast<CliMenuCommand>().ToList();
    }
}
