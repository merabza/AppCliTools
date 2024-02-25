using System;
using CliMenu;
using CliParametersEdit.Generators;
using LibDataInput;
using LibFileParameters.Interfaces;
using LibParameters;
using SystemToolsShared;

namespace CliParametersEdit.MenuCommands;

public sealed class GenerateArchiversCommand : CliMenuCommand
{
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GenerateArchiversCommand(IParametersManager parametersManager)
    {
        _parametersManager = parametersManager;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.Reload;
        var parameters = (IParametersWithArchivers)_parametersManager.Parameters;
        try
        {
            if (!Inputer.InputBool("This process will change Archivers, are you sure?", false, false))
                return;

            StandardArchiversGenerator.Generate(true, _parametersManager);

            //შენახვა
            _parametersManager.Save(parameters, "Archivers generated success");
        }
        catch (DataInputEscapeException)
        {
            Console.WriteLine();
            Console.WriteLine("Escape... ");
            StShared.Pause();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, true);
        }
    }
}