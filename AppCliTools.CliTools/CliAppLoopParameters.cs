using AppCliTools.CliTools.Services.MenuBuilder;
using AppCliTools.CliTools.Services.RecentCommands;
using Microsoft.Extensions.DependencyInjection;
using SystemTools.BackgroundTasks;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliTools;

public class CliAppLoopParameters
{
    public IRecentCommandsService RecentCommandsService { get; private init; }
    public IMenuBuilder MenuBuilder { get; private init; }
    public IApplication App { get; private init; }
    public IProcesses? Processes { get; private init; }

    public static CliAppLoopParameters? Create(ServiceProvider serviceProvider)
    {
        var app = serviceProvider.GetService<IApplication>();
        if (app is null)
        {
            StShared.WriteErrorLine("app is null", true);
            return null;
        }

        var menuBuilder = serviceProvider.GetService<IMenuBuilder>();
        if (menuBuilder is null)
        {
            StShared.WriteErrorLine("menuBuilder is null", true);
            return null;
        }

        var recentCommandsService = serviceProvider.GetService<IRecentCommandsService>();
        if (recentCommandsService is null)
        {
            StShared.WriteErrorLine("recentCommandsService is null", true);
            return null;
        }

        var processes = serviceProvider.GetService<IProcesses>();

        return new CliAppLoopParameters
        {
            App = app,
            MenuBuilder = menuBuilder,
            RecentCommandsService = recentCommandsService,
            Processes = processes
        };
    }
}
