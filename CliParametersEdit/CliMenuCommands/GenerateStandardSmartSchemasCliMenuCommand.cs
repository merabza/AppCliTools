using CliMenu;
using CliParametersEdit.Generators;
using LibDataInput;
using LibFileParameters.Interfaces;
using LibParameters;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParametersEdit.CliMenuCommands;

public sealed class GenerateStandardSmartSchemasCliMenuCommand : CliMenuCommand
{
    private readonly IParametersManager _parametersManager;

    public GenerateStandardSmartSchemasCliMenuCommand(IParametersManager parametersManager)
    {
        _parametersManager = parametersManager;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.Reload;
        var parameters = (IParametersWithSmartSchemas)_parametersManager.Parameters;

        if (!Inputer.InputBool("This process will change Smart Schemas, are you sure?", false, false))
            return;

        StandardSmartSchemas.Generate(_parametersManager);

        //შენახვა
        _parametersManager.Save(parameters, "Smart Schemas generated success");
    }
}