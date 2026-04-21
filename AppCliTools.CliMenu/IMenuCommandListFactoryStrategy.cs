using System.Collections.Generic;

namespace AppCliTools.CliMenu;

public interface IMenuCommandListFactoryStrategy
{
    string StrategyName { get; }
    List<CliMenuCommand> CreateMenuCommandsList();
}
