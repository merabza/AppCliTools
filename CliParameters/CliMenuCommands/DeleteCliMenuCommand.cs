using CliMenu;
using SystemToolsShared;

namespace CliParameters.CliMenuCommands;

public sealed class DeleteCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteCliMenuCommand(Cruder cruder, string fileStorageName) : base("Delete", fileStorageName)
    {
        _cruder = cruder;
    }

    protected override void RunAction()
    {
        if (RunBody())
            return;
        MenuAction = EMenuAction.Reload;
    }

    private bool RunBody()
    {
        if (string.IsNullOrWhiteSpace(ParentMenuName))
        {
            StShared.WriteErrorLine("Empty Parent Menu Name ", true);
            return false;
        }

        if (!_cruder.DeleteRecord(ParentMenuName))
            return false;

        MenuAction = EMenuAction.LevelUp;
        return true;
    }
}