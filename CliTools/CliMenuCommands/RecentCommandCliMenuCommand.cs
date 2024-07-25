using CliMenu;

namespace CliTools.CliMenuCommands;

public class RecentCommandCliMenuCommand : InfoCliMenuCommand
{
    private readonly CliAppLoop _cliAppLoop;
    private readonly string _menuLinkWithoutMainMenu;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RecentCommandCliMenuCommand(CliAppLoop cliAppLoop, string menuLinkWithoutMainMenu) : base(menuLinkWithoutMainMenu, menuLinkWithoutMainMenu)
    {
        _cliAppLoop = cliAppLoop;
        _menuLinkWithoutMainMenu = menuLinkWithoutMainMenu;
    }

    protected override bool RunBody()
    {
        var menuLine = _menuLinkWithoutMainMenu.Split('/');

        var currentMenu = _cliAppLoop.BuildMainMenu();

        CliMenuItem? menuItem = null;
        foreach (var menuName in menuLine)
        {
            menuItem = currentMenu?.GetMenuItemWithName(menuName);
            if (menuItem is null)
                return false;

            currentMenu = menuItem.CliMenuCommand.GetSubmenu();
        }

        if (menuItem is null) 
            return false;

        MenuActionOnBodySuccess = menuItem.CliMenuCommand.MenuActionOnBodySuccess;
        MenuActionOnBodyFail = menuItem.CliMenuCommand.MenuActionOnBodyFail;

        menuItem.CliMenuCommand.Run();
        //MenuAction = menuItem.CliMenuCommand.MenuAction;
        return true;

    }
}