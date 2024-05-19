using CliMenu;
using SystemToolsShared;

namespace CliParameters.CliMenuCommands;

public sealed class DeleteCruderRecordCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteCruderRecordCliMenuCommand(Cruder cruder, string recordName) : base("Delete", EMenuAction.LevelUp, EMenuAction.Reload, recordName)
    {
        _cruder = cruder;
    }

    protected override bool RunBody()
    {
        if (!string.IsNullOrWhiteSpace(ParentMenuName)) 
            return _cruder.DeleteRecord(ParentMenuName);

        StShared.WriteErrorLine("Empty Parent Menu Name ", true);
        return false;

    }
}