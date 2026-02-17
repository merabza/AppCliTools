using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.CliMenuCommands;

public sealed class EditItemAllFieldsInSequenceCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EditItemAllFieldsInSequenceCliMenuCommand(Cruder cruder, string itemName) : base(
        "Edit All fields in sequence", EMenuAction.LevelUp, EMenuAction.Reload, itemName)
    {
        _cruder = cruder;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(ParentMenuName))
        {
            return ValueTask.FromResult(_cruder.EditItemAllFieldsInSequence(ParentMenuName));
        }

        StShared.WriteErrorLine("Empty Parent Menu Name ", true);
        return ValueTask.FromResult(false);
    }
}
