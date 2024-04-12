using CliMenu;

namespace CliParameters.MenuCommands;

//გამოიყენება ApAgent-ში
public sealed class CruderListCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CruderListCommand(Cruder cruder) : base(cruder.CrudNamePlural)
    {
        _cruder = cruder;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.LoadSubMenu;
    }

    public override CliMenuSet GetSubmenu()
    {
        return _cruder.GetListMenu();
    }

    protected override string GetStatus()
    {
        return _cruder.Count.ToString();
    }
}