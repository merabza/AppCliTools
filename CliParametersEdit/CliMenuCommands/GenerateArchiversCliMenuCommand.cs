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
    public GenerateArchiversCliMenuCommand(IParametersManager parametersManager) : base(null, EMenuAction.Reload)
    {
        _parametersManager = parametersManager;
    }

    protected override bool RunBody()
    {
        var parameters = (IParametersWithArchivers)_parametersManager.Parameters;

        if (!Inputer.InputBool("This process will change Archivers, are you sure?", false, false))
            return false;

        StandardArchiversGenerator.Generate(true, _parametersManager);

        //შენახვა
        _parametersManager.Save(parameters, "Archivers generated success");
        return true;
    }
}