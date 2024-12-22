using System;
using System.Threading;
using System.Threading.Tasks;
using CliParameters;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibSeedCodeCreator;

public sealed class CreatorCreator : ToolCommand
{
    private const string ActionName = "Create seeder Projects code";

    private readonly ILogger _logger;

    public CreatorCreator(ILogger logger, CreatorCreatorParameters parametersManager) : base(logger, ActionName,
        parametersManager, null)
    {
        _logger = logger;
    }

    private CreatorCreatorParameters? Parameters => Par as CreatorCreatorParameters;

    protected override ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        if (Parameters is null)
            throw new Exception("Parameters is null in CreatorCreator");
        var createProjectSeederCodeProgramCreator =
            new CreateProjectSeederCodeProgramCreator(Parameters, _logger);
        createProjectSeederCodeProgramCreator.Go();

        var getJsonFromProjectDbProgramCreator =
            new GetJsonFromProjectDbProgramCreator(Parameters, _logger);
        getJsonFromProjectDbProgramCreator.Go();

        var seedProjectDbProgramCreator =
            new SeedProjectDbProgramCreator(Parameters, _logger);
        seedProjectDbProgramCreator.Go();

        return ValueTask.FromResult(true);
    }
}