using System;
using System.IO;
using System.Linq;
using DbContextAnalyzer.CodeCreators;
using DbContextAnalyzer.Domain;
using DbContextAnalyzer.Models;
using JetBrainsResharperGlobalToolsWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DbContextAnalyzer;

public sealed class SeederCodeCreator
{
    private readonly DbContext _carcassContext;
    private readonly DbContext _dbContext;

    private readonly ExcludesRulesParametersDomain _excludesRulesParameters;
    private readonly GetJsonCreatorParameters _getJsonCreatorParameters;

    private readonly ILogger _logger;
    private readonly SeederCodeCreatorParameters _seederCodeCreatorParameters;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SeederCodeCreator(ILogger logger, DbContext carcassContext, DbContext dbContext,
        GetJsonCreatorParameters getJsonCreatorParameters, SeederCodeCreatorParameters seederCodeCreatorParameters,
        ExcludesRulesParametersDomain excludesRulesParameters)
    {
        _logger = logger;
        _carcassContext = carcassContext;
        _dbContext = dbContext;
        _getJsonCreatorParameters = getJsonCreatorParameters;
        _seederCodeCreatorParameters = seederCodeCreatorParameters;
        _excludesRulesParameters = excludesRulesParameters;
    }

    public void Go()
    {
        //0.1
        var dataSeederBaseGenericClassCreator =
            new DataSeederBaseGenericClassCreator(_logger, _seederCodeCreatorParameters);
        dataSeederBaseGenericClassCreator.CreateFileStructure();

        //0.2
        var dataSeederRepositoryInterfaceCreator =
            new DataSeederRepositoryInterfaceCreator(_logger, _seederCodeCreatorParameters);
        dataSeederRepositoryInterfaceCreator.CreateFileStructure();

        //0.3
        var dataSeederRepositoryCreator = new DataSeederRepositoryCreator(_logger, _seederCodeCreatorParameters);
        dataSeederRepositoryCreator.CreateFileStructure();

        //1.1
        var creatorForJsonFilesCreator =
            new CreatorForJsonFilesCreator(_logger, _getJsonCreatorParameters, _excludesRulesParameters);
        creatorForJsonFilesCreator.CreateFileStructure();

        //1.2
        var projectDataSeederCreator = new ProjectDataSeederCreator(_logger, _seederCodeCreatorParameters);
        projectDataSeederCreator.CreateFileStructure();

        //----
        var carcassEntityTypes = _carcassContext.Model.GetEntityTypes().ToList();
        var relations = new Relations(_dbContext, _excludesRulesParameters);
        relations.DbContextAnalysis();
        //-----

        //-------------

        var isAnyCarcassType = false;
        foreach (var relEntity in relations.Entities.OrderBy(o => o.Value.Level).ThenBy(tb => tb.Key))
        {
            var tableName = relEntity.Key;

            var isCarcassType = carcassEntityTypes.Any(a => string.Equals(Relations.GetTableName(a)?.ToLower(),
                tableName.ToLower(), StringComparison.OrdinalIgnoreCase));
            if (isCarcassType)
                isAnyCarcassType = true;
        }
        //-------------

        //1.3
        var projectDataSeedersFabricCreator =
            new ProjectDataSeedersFabricCreator(_logger, _seederCodeCreatorParameters, isAnyCarcassType);
        projectDataSeedersFabricCreator.CreateFileStructure();

        //-------------

        var lastLevel = -1;
        foreach (var relEntity in relations.Entities.OrderBy(o => o.Value.Level).ThenBy(tb => tb.Key))
        {
            if (relEntity.Value.Level != lastLevel)
            {
                lastLevel = relEntity.Value.Level;
                _logger.LogInformation("Level = {lastLevel}", lastLevel);
            }

            var tableName = relEntity.Key;
            _logger.LogInformation("TableName = {tableName}", tableName);

            var isCarcassType = carcassEntityTypes.Any(a =>
                string.Equals(Relations.GetTableName(a), tableName, StringComparison.OrdinalIgnoreCase));
            //2.1
            var seederModelCreatorForJsonCreatorProject = new SeederModelCreator(_logger,
                _getJsonCreatorParameters.PlacePath, _getJsonCreatorParameters.ProjectNamespace,
                _getJsonCreatorParameters.ModelsFolderName, _excludesRulesParameters.SingularityExceptions);
            seederModelCreatorForJsonCreatorProject.UseEntity(relEntity.Value);

            if (!isCarcassType)
            {
                //2.2
                var seederModelCreator = new SeederModelCreator(_logger, _seederCodeCreatorParameters.PlacePath,
                    _seederCodeCreatorParameters.ProjectNamespace, _seederCodeCreatorParameters.ModelsFolderName,
                    _excludesRulesParameters.SingularityExceptions);
                seederModelCreator.UseEntity(relEntity.Value);
            }

            //2.3
            var placePath = Path.Combine(_seederCodeCreatorParameters.PlacePath,
                isCarcassType
                    ? _seederCodeCreatorParameters.CarcassSeedersFolderName
                    : _seederCodeCreatorParameters.ProjectSeedersFolderName);

            var seederCreator =
                new SeederCreator(_logger, _seederCodeCreatorParameters, _excludesRulesParameters, placePath);
            seederCreator.UseEntity(relEntity.Value, isCarcassType);

            //1.1
            creatorForJsonFilesCreator.UseEntity(relEntity.Value);
            //1.2
            if (!isCarcassType)
                projectDataSeederCreator.UseEntity(relEntity.Value);

            //1.3
            projectDataSeedersFabricCreator.UseEntity(relEntity.Value, isCarcassType);
        }
        //-------------

        //1.1
        creatorForJsonFilesCreator.FinishAndSave();
        //1.2
        projectDataSeederCreator.FinishAndSave();
        //1.3
        projectDataSeedersFabricCreator.FinishAndSave();

        var place = new DirectoryInfo(_getJsonCreatorParameters.PlacePath);
        var solutionDir = place.Parent?.Parent;
        if (solutionDir is null)
            return;

        var processor = new JetBrainsResharperGlobalToolsProcessor(_logger, true);
        processor.Cleanupcode(solutionDir.FullName);
    }
}