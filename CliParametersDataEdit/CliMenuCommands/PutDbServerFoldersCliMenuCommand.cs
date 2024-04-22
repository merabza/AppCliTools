using CliMenu;
using LibDataInput;
using LibParameters;
using System;
using System.Threading;
using CliParametersDataEdit.ToolActions;
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
        try
        {
            var putDbServerFoldersToolAction =
                new PutDbServerFoldersToolAction(_logger, _dbServerName, _parametersManager);
            putDbServerFoldersToolAction.Run(CancellationToken.None).Wait();
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