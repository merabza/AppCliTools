using CliMenu;
using LibDatabaseParameters;
using LibDataInput;
using LibParameters;
using System;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace CliParametersDataEdit.CliMenuCommands;

public class PutDbServerFoldersCliMenuCommand : CliMenuCommand
{
    private readonly ILogger _logger;
    private readonly string _dbServerName;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public PutDbServerFoldersCliMenuCommand(ILogger logger, string dbServerName, IParametersManager parametersManager)
    {
        _logger = logger;
        _dbServerName = dbServerName;
        _parametersManager = parametersManager;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.Reload;
        var parameters = (IParametersWithDatabaseServerConnections)_parametersManager.Parameters;
        try
        {
            //if (!Inputer.InputBool("This process will change Environments, are you sure?", false, false))
            //    return;

            //StandardEnvironmentsGenerator.Generate(_parametersManager);

            ////შენახვა
            //_parametersManager.Save(parameters, "Environments generated success");
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