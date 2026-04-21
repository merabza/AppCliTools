using System.Collections.Generic;

namespace AppCliTools.CliMenu;

public interface IMenuCommandListFactoryStrategy
{
    List<CliMenuCommand> CreateMenuCommandsList();
}
