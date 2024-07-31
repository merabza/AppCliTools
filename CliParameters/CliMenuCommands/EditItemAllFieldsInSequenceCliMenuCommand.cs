using CliMenu;
using SystemToolsShared;

namespace CliParameters.CliMenuCommands;

public sealed class EditItemAllFieldsInSequenceCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EditItemAllFieldsInSequenceCliMenuCommand(Cruder cruder, string itemName) : base(
        "Edit All fields in sequence", EMenuAction.LevelUp, EMenuAction.Reload, itemName)
    {
        _cruder = cruder;
    }

    protected override bool RunBody()
    {
        if (!string.IsNullOrWhiteSpace(ParentMenuName))
            return _cruder.EditItemAllFieldsInSequence(ParentMenuName);

        StShared.WriteErrorLine("Empty Parent Menu Name ", true);
        return false;
    }
}