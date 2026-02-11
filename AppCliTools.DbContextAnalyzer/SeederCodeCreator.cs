using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppCliTools.CodeTools;
using AppCliTools.DbContextAnalyzer.CodeCreators;
using AppCliTools.DbContextAnalyzer.Domain;
using AppCliTools.DbContextAnalyzer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using SystemTools.JetBrainsResharperGlobalToolsWork;

namespace AppCliTools.DbContextAnalyzer;

public sealed class SeederCodeCreator
{
    private readonly DbContext _carcassContext;
    private readonly DbContext _dbScContext;
    private readonly DbContext _devContext;

    private readonly ExcludesRulesParametersDomain _excludesRulesParameters;
    private readonly GetJsonCreatorParameters _getJsonCreatorParameters;

    private readonly ILogger _logger;
    private readonly SeederCodeCreatorParameters _seederCodeCreatorParameters;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SeederCodeCreator(ILogger logger, DbContext carcassContext, DbContext dbScContext, DbContext devContext,
        GetJsonCreatorParameters getJsonCreatorParameters, SeederCodeCreatorParameters seederCodeCreatorParameters,
        ExcludesRulesParametersDomain excludesRulesParameters)
    {
        _logger = logger;
        _carcassContext = carcassContext;
        _dbScContext = dbScContext;
        _devContext = devContext;
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
        var projectDataSeederCreator =
            new ProjectDataSeederCreator(_logger, _seederCodeCreatorParameters, _excludesRulesParameters);
        projectDataSeederCreator.CreateFileStructure();

        //----
        List<IEntityType> carcassEntityTypes = _carcassContext.Model.GetEntityTypes().ToList();

        var relations = new Relations(_dbScContext, _excludesRulesParameters);
        relations.DbContextAnalysis();

        var relationsInDevBase = new Relations(_devContext, _excludesRulesParameters);
        relationsInDevBase.DbContextAnalysis();
        //-----

        var usedCarcassEntityTypes = new List<string>();

        //-------------

        //var isAnyCarcassType = false;
        //foreach (var relEntity in relations.Entities.OrderBy(o => o.Value.Level).ThenBy(tb => tb.Key))
        //{
        //    var tableName = relEntity.Key;

        //    var isCarcassType = carcassEntityTypes.Any(a => string.Equals(Relations.GetTableName(a)?.ToLower(),
        //        tableName.ToLower(), StringComparison.OrdinalIgnoreCase));
        //    if (isCarcassType)
        //        isAnyCarcassType = true;
        //}
        //-------------

        //1.3
        var projectDataSeedersFactoryCreator =
            new ProjectDataSeedersFactoryCreator(_logger, _seederCodeCreatorParameters, _excludesRulesParameters);
        projectDataSeedersFactoryCreator.CreateFileStructure();

        //-------------

        int lastLevel = -1;
        foreach ((string tableName, EntityData value) in relations.Entities.OrderBy(o => o.Value.Level)
                     .ThenBy(tb => tb.Key))
        {
            if (value.Level != lastLevel)
            {
                lastLevel = value.Level;
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Level = {LastLevel}", lastLevel);
                }
            }

            string newTableName = _excludesRulesParameters.GetReplaceTablesName(tableName);
            KeyValuePair<string, EntityData> devBaseTableEntity = relationsInDevBase.Entities.SingleOrDefault(x =>
                string.Equals(x.Value.TableName, newTableName, StringComparison.OrdinalIgnoreCase));

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("TableName = {TableName}", tableName);
            }

            bool isCarcassType = carcassEntityTypes.Any(a =>
                string.Equals(Relations.GetTableName(a), tableName, StringComparison.OrdinalIgnoreCase));
            //2.1
            var seederModelCreatorForJsonCreatorProject = new SeederModelCreator(_logger,
                _getJsonCreatorParameters.PlacePath, _getJsonCreatorParameters.ProjectNamespace,
                _getJsonCreatorParameters.ModelsFolderName, _excludesRulesParameters);
            seederModelCreatorForJsonCreatorProject.UseEntity(value);

            if (!isCarcassType)
            {
                //2.2
                var seederModelCreator = new SeederModelCreator(_logger, _seederCodeCreatorParameters.PlacePath,
                    _seederCodeCreatorParameters.ProjectNamespace, _seederCodeCreatorParameters.ModelsFolderName,
                    _excludesRulesParameters);
                seederModelCreator.UseEntity(value);
            }

            //2.3
            string placePath = Path.Combine(_seederCodeCreatorParameters.PlacePath,
                isCarcassType
                    ? _seederCodeCreatorParameters.CarcassSeedersFolderName
                    : _seederCodeCreatorParameters.ProjectSeedersFolderName);

            var seederCreator =
                new SeederCreator(_logger, _seederCodeCreatorParameters, _excludesRulesParameters, placePath);
            string usedTableName = seederCreator.UseEntity(value, devBaseTableEntity.Value, isCarcassType);
            if (isCarcassType)
            {
                usedCarcassEntityTypes.Add(usedTableName);
            }

            //1.1
            creatorForJsonFilesCreator.UseEntity(value);
            //1.2
            if (!isCarcassType)
            {
                projectDataSeederCreator.UseEntity(value);
            }

            //1.3
            projectDataSeedersFactoryCreator.UseEntity(value, isCarcassType);
        }
        //-------------

        List<IEntityType> notUsedCarcassTypes = (from carcassEntityType in carcassEntityTypes
            let tableName = Relations.GetTableName(carcassEntityType)
            where !string.IsNullOrEmpty(tableName)
            let isUsed =
                usedCarcassEntityTypes.Any(a => string.Equals(a, tableName, StringComparison.OrdinalIgnoreCase))
            where !isUsed
            select carcassEntityType).ToList();

        foreach (IEntityType carcassEntityType in notUsedCarcassTypes)
        {
            //2.3
            string placePath = Path.Combine(_seederCodeCreatorParameters.PlacePath,
                _seederCodeCreatorParameters.CarcassSeedersFolderName);

            var seederCreator =
                new SeederCreator(_logger, _seederCodeCreatorParameters, _excludesRulesParameters, placePath);
            seederCreator.UseCarcassEntity(carcassEntityType);
        }

        //1.1
        creatorForJsonFilesCreator.FinishAndSave();
        //1.2
        projectDataSeederCreator.FinishAndSave();
        //1.3
        projectDataSeedersFactoryCreator.FinishAndSave();

        var place = new DirectoryInfo(_getJsonCreatorParameters.PlacePath);
        DirectoryInfo? solutionDir = place.Parent?.Parent;
        if (solutionDir is null)
        {
            return;
        }

        var processor = new JetBrainsResharperGlobalToolsProcessor(_logger, true);
        processor.Cleanupcode(solutionDir.FullName);
    }
}
