using System.Threading.Tasks;
using AppCliTools.CliMenu;

namespace AppCliTools.CliTools.Services.MenuBuilder;

public interface IMenuBuilder
{
    Task<CliMenuSet?> BuildMainMenu();
}
