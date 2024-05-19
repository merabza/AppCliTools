using System.Threading;
using CliMenu;
using CliParametersDataEdit.ToolActions;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersDataEdit.CliMenuCommands;

public class GetDbServerFoldersCliMenuCommand : CliMenuCommand
{
    private readonly string _dbServerName;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDbServerFoldersCliMenuCommand(ILogger logger, string dbServerName, IParametersManager parametersManager):base(null,EMenuAction.Reload)
    {
        _logger = logger;
        _dbServerName = dbServerName;
        _parametersManager = parametersManager;
    }

    protected override bool RunBody()
    {
        var getDbServerFoldersToolAction = new GetDbServerFoldersToolAction(_logger, _dbServerName, _parametersManager);
        return getDbServerFoldersToolAction.Run(CancellationToken.None).Result;
    }
}