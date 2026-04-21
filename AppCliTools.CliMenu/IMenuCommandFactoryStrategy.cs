namespace AppCliTools.CliMenu;

public interface IMenuCommandFactoryStrategy
{
    //string StrategyName { get; }
    CliMenuCommand CreateMenuCommand();
}
