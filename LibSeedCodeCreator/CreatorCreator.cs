using System;
using CliParameters;
using DbContextAnalyzer.Models;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace LibSeedCodeCreator;

public sealed class CreatorCreator : ToolCommand
{
    private const string ActionName = "Create seeder Projects code";

    private readonly ILogger _logger;

    public CreatorCreator(ILogger logger, ParametersManager parametersManager) : base(logger, ActionName,
        parametersManager)
    {
        _logger = logger;
    }

    private CreatorCreatorParameters? Parameters => Par as CreatorCreatorParameters;

    protected override bool RunAction()
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

        return true;
    }
}