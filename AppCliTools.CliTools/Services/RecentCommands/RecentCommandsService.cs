using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliTools.CliMenuCommands;
using AppCliTools.CliTools.Services.MenuBuilder;
using AppCliTools.CliTools.Services.RecentCommands.Models;
using Microsoft.Extensions.Options;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliTools.Services.RecentCommands;

public class RecentCommandsService : IRecentCommandsService
{
    private readonly IMenuBuilder _menuBuilder;
    private readonly int _recentCommandsCount;
    private readonly string? _recentCommandsFileName;
    private RecentCommandsModel _recentCommands = new();

    // ReSharper disable once ConvertToPrimaryConstructor
    public RecentCommandsService(IMenuBuilder menuBuilder, IOptions<RecentCommandOptions> options)
    {
        _menuBuilder = menuBuilder;
        _recentCommandsFileName = options.Value.RecentCommandsFileName;
        _recentCommandsCount = options.Value.RecentCommandsCount;
    }

    public IEnumerable<RecentCommandCliMenuCommand> GetRecentCommands()
    {
        return _recentCommands.Rc.OrderByDescending(x => x.Value)
            .Select(rcKvp => new RecentCommandCliMenuCommand(_menuBuilder, rcKvp.Key));
    }

    public void LoadRecent()
    {
        if (string.IsNullOrWhiteSpace(_recentCommandsFileName) || _recentCommandsCount < 1)
        {
            return;
        }

        var parLoader = new ParametersLoader<RecentCommandsModel>();
        if (!parLoader.TryLoadParameters(_recentCommandsFileName, false) || parLoader.Par is null)
        {
            return;
        }

        _recentCommands = (RecentCommandsModel)parLoader.Par;
        _recentCommands.Rc = _recentCommands.Rc.OrderByDescending(x => x.Key).ToDictionary(k => k.Key, v => v.Value);
    }

    public async ValueTask SaveRecent(CliMenuCommand menuCommand)
    {
        if (string.IsNullOrWhiteSpace(_recentCommandsFileName) || _recentCommandsCount < 1)
        {
            return;
        }

        string? commLink = menuCommand.CommandLink;
        if (commLink is null)
        {
            return;
        }

        _recentCommands.Rc[commLink] = DateTime.Now;

        _recentCommands.Rc = _recentCommands.Rc.OrderByDescending(x => x.Value).ToDictionary(k => k.Key, v => v.Value);

        if (_recentCommands.Rc.Count > _recentCommandsCount)
        {
            _recentCommands.Rc = _recentCommands.Rc.OrderByDescending(x => x.Value).Take(_recentCommandsCount)
                .ToDictionary(k => k.Key, v => v.Value);
        }

        var parMan = new ParametersManager(_recentCommandsFileName, _recentCommands);
        await parMan.Save(_recentCommands, null);
    }
}
