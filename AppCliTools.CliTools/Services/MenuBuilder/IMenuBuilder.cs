using AppCliTools.CliMenu;

namespace AppCliTools.CliTools.Services.MenuBuilder;

public interface IMenuBuilder
{
    CliMenuSet? BuildMainMenu();
}
