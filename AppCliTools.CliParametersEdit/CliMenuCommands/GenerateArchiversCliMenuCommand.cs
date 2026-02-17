using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersEdit.Generators;
using AppCliTools.LibDataInput;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersEdit.CliMenuCommands;

public sealed class GenerateArchiversCliMenuCommand : CliMenuCommand
{
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GenerateArchiversCliMenuCommand(IParametersManager parametersManager) : base(
        "Generate standard Archivers...", EMenuAction.Reload)
    {
        _parametersManager = parametersManager;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var parameters = (IParametersWithArchivers)_parametersManager.Parameters;

        if (!Inputer.InputBool("This process will change Archivers, are you sure?", false, false))
        {
            return ValueTask.FromResult(false);
        }

        StandardArchiversGenerator.Generate(true, _parametersManager);

        //შენახვა
        _parametersManager.Save(parameters, "Archivers generated success");
        return ValueTask.FromResult(true);
    }
}
