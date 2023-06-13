using System;
using CliMenu;
using CliParametersExcludeSetsEdit.Generators;
using LibDataInput;
using LibFileParameters.Interfaces;
using LibParameters;
using SystemToolsShared;

namespace CliParametersExcludeSetsEdit.MenuCommands;

public sealed class GenerateExcludeSetsCommand : CliMenuCommand
{
    private readonly IParametersManager _parametersManager;

    public GenerateExcludeSetsCommand(IParametersManager parametersManager)
    {
        _parametersManager = parametersManager;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.Reload;
        var parameters = (IParametersWithExcludeSets)_parametersManager.Parameters;
        try
        {
            if (!Inputer.InputBool("This process will change Exclude Sets, are you sure?", false, false))
                return;

            StandardExcludeSetsGenerator standardExcludeSetsGenerator = new(_parametersManager);
            standardExcludeSetsGenerator.Generate();

            //შენახვა
            _parametersManager.Save(parameters, "ExcludeSets generated success");
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