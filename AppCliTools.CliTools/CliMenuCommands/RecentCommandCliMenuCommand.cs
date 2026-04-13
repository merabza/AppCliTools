using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliTools.Services.MenuBuilder;

namespace AppCliTools.CliTools.CliMenuCommands;

public sealed class RecentCommandCliMenuCommand : InfoCliMenuCommand
{
    private readonly IMenuBuilder _menuBuilder;
    private readonly string _menuLinkWithoutMainMenu;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RecentCommandCliMenuCommand(IMenuBuilder menuBuilder, string menuLinkWithoutMainMenu) : base(
        menuLinkWithoutMainMenu, menuLinkWithoutMainMenu)
    {
        _menuBuilder = menuBuilder;
        _menuLinkWithoutMainMenu = menuLinkWithoutMainMenu;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        string[] menuLine = _menuLinkWithoutMainMenu.Split('/');

        CliMenuSet? currentMenu = _menuBuilder.BuildMainMenu();

        CliMenuItem? menuItem = null;
        foreach (string menuName in menuLine)
        {
            menuItem = currentMenu?.GetMenuItemWithName(menuName);
            if (menuItem is null)
            {
                return false;
            }

            currentMenu = menuItem.CliMenuCommand.GetSubMenu();
        }

        if (menuItem is null)
        {
            return false;
        }

        MenuActionOnBodySuccess = menuItem.CliMenuCommand.MenuActionOnBodySuccess;
        MenuActionOnBodyFail = menuItem.CliMenuCommand.MenuActionOnBodyFail;

        await menuItem.CliMenuCommand.Run(cancellationToken);
        //MenuAction = menuItem.CliMenuCommand.MenuAction;
        return true;
    }
}
