using AppCliTools.CliMenu;

namespace AppCliTools.CliTools.CliMenuCommands;

public /*open*/ class InfoCliMenuCommand : CliMenuCommand
{
    private readonly string _menuLink;

    
    public InfoCliMenuCommand(string info, string menuLink) : base(info, EMenuAction.GoToMenuLink,
        EMenuAction.GoToMenuLink, null, false, EStatusView.Brackets, true)
    {
        _menuLink = menuLink;
    }

    public override string GetMenuLinkToGo()
    {
        return _menuLink;
    }
}
