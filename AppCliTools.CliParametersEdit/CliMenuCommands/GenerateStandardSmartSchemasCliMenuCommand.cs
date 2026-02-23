using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersEdit.Generators;
using AppCliTools.LibDataInput;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersEdit.CliMenuCommands;

public sealed class GenerateStandardSmartSchemasCliMenuCommand : CliMenuCommand
{
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GenerateStandardSmartSchemasCliMenuCommand(IParametersManager parametersManager) : base(
        "Generate standard Smart Schemas...", EMenuAction.Reload)
    {
        _parametersManager = parametersManager;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var parameters = (IParametersWithSmartSchemas)_parametersManager.Parameters;

        if (!Inputer.InputBool("This process will change Smart Schemas, are you sure?", false, false))
        {
            return false;
        }

        StandardSmartSchemas.Generate(_parametersManager);

        //შენახვა
        await _parametersManager.Save(parameters, "Smart Schemas generated success", null, cancellationToken);
        return true;
    }
}
