using AppCliTools.CliTools.Services.MenuBuilder;
using AppCliTools.CliTools.Services.RecentCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SystemTools.BackgroundTasks;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliTools;

public class CliAppLoopParameters
{
    public IRecentCommandsService? RecentCommandsService { get; private init; }
    public IMenuBuilder MenuBuilder { get; private init; }
    public IApplication App { get; private init; }
    public IProcesses? Processes { get; private init; }

    public static (CliAppLoopParameters?, ILogger<T>?) Create<T>(ServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<T>>();
        if (logger is null)
        {
            StShared.WriteErrorLine("logger is null", true);
            return (null, null);
        }

        var app = serviceProvider.GetService<IApplication>();
        if (app is null)
        {
            StShared.WriteErrorLine("app is null", true);
            return (null, logger);
        }

        var menuBuilder = serviceProvider.GetService<IMenuBuilder>();
        if (menuBuilder is null)
        {
            StShared.WriteErrorLine("menuBuilder is null", true);
            return (null, logger);
        }

        var recentCommandsService = serviceProvider.GetService<IRecentCommandsService>();

        var processes = serviceProvider.GetService<IProcesses>();

        return (
            new CliAppLoopParameters
            {
                App = app,
                MenuBuilder = menuBuilder,
                RecentCommandsService = recentCommandsService,
                Processes = processes
            }, logger);
    }
}
