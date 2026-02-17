using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;

namespace AppCliTools.CliParameters.CliMenuCommands;

public sealed class NewItemCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public NewItemCliMenuCommand(Cruder cruder, string parentMenuName, string commandName) : base(commandName,
        EMenuAction.Reload, EMenuAction.Reload, parentMenuName)
    {
        _cruder = cruder;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        MenuAction = EMenuAction.Reload;
        return ValueTask.FromResult(_cruder.CreateNewRecord() is not null);
    }
}
