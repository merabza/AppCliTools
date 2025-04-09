using System.Net.Http;
using System.Threading;
using CliMenu;
using CliParametersDataEdit.ToolActions;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersDataEdit.CliMenuCommands;

public sealed class GetDbServerFoldersCliMenuCommand : CliMenuCommand
{
    private readonly string _dbServerName;
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDbServerFoldersCliMenuCommand(ILogger logger, IHttpClientFactory? httpClientFactory, string dbServerName,
        IParametersManager parametersManager) : base("Get Database Server Folders and save in parameters",
        EMenuAction.Reload)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _dbServerName = dbServerName;
        _parametersManager = parametersManager;
    }

    protected override bool RunBody()
    {
        var getDbServerFoldersToolAction =
            new GetDbServerFoldersToolAction(_logger, _httpClientFactory, _dbServerName, _parametersManager);
        return getDbServerFoldersToolAction.Run(CancellationToken.None).Result;
    }
}