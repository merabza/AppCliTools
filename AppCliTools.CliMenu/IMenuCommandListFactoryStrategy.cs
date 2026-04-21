using System.Collections.Generic;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliMenu;

public interface IMenuCommandListFactoryStrategy
{
    string StrategyName { get; }
    List<CliMenuCommand> CreateMenuCommandsList(IParametersManager parametersManager);
}
