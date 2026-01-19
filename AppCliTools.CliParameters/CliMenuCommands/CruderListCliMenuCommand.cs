using System.Globalization;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;

namespace AppCliTools.CliParameters.CliMenuCommands;

//გამოიყენება ApAgent-ში
// ReSharper disable once UnusedType.Global
public sealed class CruderListCliMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CruderListCliMenuCommand(Cruder cruder) : base(cruder.CrudNamePlural, EMenuAction.LoadSubMenu)
    {
        _cruder = cruder;
    }

    public override CliMenuSet GetSubMenu()
    {
        return _cruder.GetListMenu();
    }

    protected override string GetStatus()
    {
        return _cruder.Count.ToString(CultureInfo.InvariantCulture);
    }
}
