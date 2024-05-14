using CliMenu;
using CliParametersDataEdit.ToolActions;
using LibParameters;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace CliParametersDataEdit.CliMenuCommands;

public class PutDbServerFoldersCliMenuCommand : CliMenuCommand
{
    private readonly string _dbServerName;
    private readonly ILogger _logger;
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

        var putDbServerFoldersToolAction = new PutDbServerFoldersToolAction(_logger, _dbServerName, _parametersManager);
        putDbServerFoldersToolAction.Run(CancellationToken.None).Wait();
    }
}