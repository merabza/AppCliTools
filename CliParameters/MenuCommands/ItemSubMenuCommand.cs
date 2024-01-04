using CliMenu;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParameters.MenuCommands;

public sealed class ItemSubMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    public ItemSubMenuCommand(Cruder cruder, string name, string parentMenuName, bool nameIsStatus = false) : base(name,
        parentMenuName, false, EStatusView.Brackets, nameIsStatus)
    {
        _cruder = cruder;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.LoadSubMenu;
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