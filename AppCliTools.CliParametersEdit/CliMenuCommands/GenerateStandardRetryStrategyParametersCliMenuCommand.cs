using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersEdit.Generators;
using AppCliTools.LibDataInput;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersEdit.CliMenuCommands;

public sealed class GenerateStandardRetryStrategyParametersCliMenuCommand : CliMenuCommand
{
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GenerateStandardRetryStrategyParametersCliMenuCommand(IParametersManager parametersManager) : base(
        "Generate standard Retry Strategy Parameters...", EMenuAction.Reload)
    {
        _parametersManager = parametersManager;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var parameters = (IParametersWithRetryStrategyParameters)_parametersManager.Parameters;

        if (!Inputer.InputBool("This process will change Retry Strategy Parameters, are you sure?", false, false))
        {
            return false;
        }

        StandardRetryStrategyParameters.Generate(_parametersManager);

        //შენახვა
        await _parametersManager.Save(parameters, "Retry Strategy Parameters generated success", null,
            cancellationToken);
        return true;
    }
}
