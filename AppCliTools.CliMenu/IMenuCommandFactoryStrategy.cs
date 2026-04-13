using ParametersManagement.LibParameters;

namespace AppCliTools.CliMenu;

public interface IMenuCommandFactoryStrategy
{
    string MenuCommandName { get; }
    CliMenuCommand CreateMenuCommand(IParametersManager parametersManager);
}
