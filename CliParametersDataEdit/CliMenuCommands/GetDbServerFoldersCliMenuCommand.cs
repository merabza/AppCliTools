using CliMenu;
using CliParametersDataEdit.ToolActions;
using LibParameters;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace CliParametersDataEdit.CliMenuCommands;

public class GetDbServerFoldersCliMenuCommand : CliMenuCommand
{
    private readonly string _dbServerName;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDbServerFoldersCliMenuCommand(ILogger logger, string dbServerName, IParametersManager parametersManager)
    {
        _logger = logger;
        _dbServerName = dbServerName;
        _parametersManager = parametersManager;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.Reload;

        var getDbServerFoldersToolAction = new GetDbServerFoldersToolAction(_logger, _dbServerName, _parametersManager);
        getDbServerFoldersToolAction.Run(CancellationToken.None).Wait();

    }
}