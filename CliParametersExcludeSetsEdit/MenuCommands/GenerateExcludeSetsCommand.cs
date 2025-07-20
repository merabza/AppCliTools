using CliMenu;
using CliParametersExcludeSetsEdit.Generators;
using LibDataInput;
using LibFileParameters.Interfaces;
using LibParameters;

namespace CliParametersExcludeSetsEdit.MenuCommands;

public sealed class GenerateExcludeSetsCommand : CliMenuCommand
{
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GenerateExcludeSetsCommand(IParametersManager parametersManager) : base("Generate standard Exclude Sets...",
        EMenuAction.Reload)
    {
        _parametersManager = parametersManager;
    }

    protected override bool RunBody()
    {
        var parameters = (IParametersWithExcludeSets)_parametersManager.Parameters;

        if (!Inputer.InputBool("This process will change Exclude Sets, are you sure?", false, false))
            return false;

        var standardExcludeSetsGenerator = new StandardExcludeSetsGenerator(_parametersManager);
        standardExcludeSetsGenerator.Generate();

        //შენახვა
        _parametersManager.Save(parameters, "ExcludeSets generated success");
        return true;
    }
}