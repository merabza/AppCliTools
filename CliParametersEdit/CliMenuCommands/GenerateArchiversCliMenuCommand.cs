using CliMenu;
using CliParametersEdit.Generators;
using LibDataInput;
using LibFileParameters.Interfaces;
using LibParameters;

namespace CliParametersEdit.CliMenuCommands;

public sealed class GenerateArchiversCliMenuCommand : CliMenuCommand
{
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GenerateArchiversCliMenuCommand(IParametersManager parametersManager)
    {
        _parametersManager = parametersManager;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.Reload;
        var parameters = (IParametersWithArchivers)_parametersManager.Parameters;

        if (!Inputer.InputBool("This process will change Archivers, are you sure?", false, false))
            return;

        StandardArchiversGenerator.Generate(true, _parametersManager);

        //შენახვა
        _parametersManager.Save(parameters, "Archivers generated success");
    }
}