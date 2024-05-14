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
    public GenerateExcludeSetsCommand(IParametersManager parametersManager)
    {
        _parametersManager = parametersManager;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.Reload;
        var parameters = (IParametersWithExcludeSets)_parametersManager.Parameters;

        if (!Inputer.InputBool("This process will change Exclude Sets, are you sure?", false, false))
            return;

        StandardExcludeSetsGenerator standardExcludeSetsGenerator = new(_parametersManager);
        standardExcludeSetsGenerator.Generate();

        //შენახვა
        _parametersManager.Save(parameters, "ExcludeSets generated success");
    }
}