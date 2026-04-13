using System.Collections.Generic;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliTools.CliMenuCommands;

namespace AppCliTools.CliTools.Services.RecentCommands;

public interface IRecentCommandsService
{
    IEnumerable<RecentCommandCliMenuCommand> GetRecentCommands();
    void LoadRecent();
    ValueTask SaveRecent(CliMenuCommand menuCommand);
}
