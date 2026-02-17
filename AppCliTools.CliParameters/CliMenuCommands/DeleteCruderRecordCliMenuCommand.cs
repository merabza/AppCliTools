using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.CliMenuCommands;

public sealed class DeleteCruderRecordCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteCruderRecordCliMenuCommand(Cruder cruder, string recordName) : base("Delete this record",
        EMenuAction.LevelUp, EMenuAction.Reload, recordName)
    {
        _cruder = cruder;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(ParentMenuName))
        {
            return ValueTask.FromResult(_cruder.DeleteRecord(ParentMenuName));
        }

        StShared.WriteErrorLine("Empty Parent Menu Name ", true);
        return ValueTask.FromResult(false);
    }
}
