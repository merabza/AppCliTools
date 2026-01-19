using System;
using System.Net.Http;
using System.Threading;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersDataEdit.ToolActions;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersDataEdit.CliMenuCommands;

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
        try
        {
            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            token.ThrowIfCancellationRequested();

            return getDbServerFoldersToolAction.Run(token).Result;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
