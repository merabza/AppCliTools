using CliMenu;
using CliParametersEdit.Generators;
using LibDataInput;
using LibFileParameters.Interfaces;
using LibParameters;

namespace CliParametersEdit.CliMenuCommands;

public sealed class GenerateStandardSmartSchemasCliMenuCommand : CliMenuCommand
{
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GenerateStandardSmartSchemasCliMenuCommand(IParametersManager parametersManager) : base(null,
        EMenuAction.Reload)
    {
        _parametersManager = parametersManager;
    }

    protected override bool RunBody()
    {
        var parameters = (IParametersWithSmartSchemas)_parametersManager.Parameters;

        if (!Inputer.InputBool("This process will change Smart Schemas, are you sure?", false, false))
            return false;

        StandardSmartSchemas.Generate(_parametersManager);

        //შენახვა
        _parametersManager.Save(parameters, "Smart Schemas generated success");
        return true;
    }
}