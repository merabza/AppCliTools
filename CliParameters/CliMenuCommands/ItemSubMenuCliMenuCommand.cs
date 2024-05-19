using CliMenu;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.CliMenuCommands;

public sealed class ItemSubMenuCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    public ItemSubMenuCliMenuCommand(Cruder cruder, string name, string parentMenuName, bool nameIsStatus = false) :
        base(name, EMenuAction.LoadSubMenu, EMenuAction.Reload, parentMenuName, false, EStatusView.Brackets,
            nameIsStatus)
    {
        _cruder = cruder;
    }

    public override CliMenuSet? GetSubmenu()
    {
        return Name == null ? null : _cruder.GetItemMenu(Name);
    }


    protected override string? GetStatus()
    {
        return Name == null ? null : _cruder.GetStatusFor(Name);
    }
}