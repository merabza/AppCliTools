using CliMenu;

namespace CliParameters.CliMenuCommands;

//გამოიყენება ApAgent-ში
public sealed class CruderListCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CruderListCliMenuCommand(Cruder cruder) : base(cruder.CrudNamePlural)
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