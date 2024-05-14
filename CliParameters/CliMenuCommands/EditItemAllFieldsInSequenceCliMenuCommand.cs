using CliMenu;
using SystemToolsShared;

namespace CliParameters.CliMenuCommands;

public sealed class EditItemAllFieldsInSequenceCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EditItemAllFieldsInSequenceCliMenuCommand(Cruder cruder, string itemName) : base("Edit", itemName)
    {
        _cruder = cruder;
    }

    protected override void RunAction()
    {
        if (string.IsNullOrWhiteSpace(ParentMenuName))
        {
            StShared.WriteErrorLine("Empty Parent Menu Name ", true);
            return;
        }

        if (_cruder.EditItemAllFieldsInSequence(ParentMenuName))
        {
            MenuAction = EMenuAction.LevelUp;
            return;
        }

        MenuAction = EMenuAction.Reload;
    }
}