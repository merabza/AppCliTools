using CliMenu;

namespace CliParameters.CliMenuCommands;

public sealed class ItemSubMenuCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ItemSubMenuCliMenuCommand(Cruder cruder, string name, string parentMenuName, bool nameIsStatus = false) :
        base(name, EMenuAction.LoadSubMenu, EMenuAction.Reload, parentMenuName, false, EStatusView.Brackets,
            nameIsStatus)
    {
        _cruder = cruder;
    }

    public override CliMenuSet GetSubMenu()
    {
        return _cruder.GetItemMenu(Name);
    }

    protected override string? GetStatus()
    {
        return _cruder.GetStatusFor(Name);
    }
}