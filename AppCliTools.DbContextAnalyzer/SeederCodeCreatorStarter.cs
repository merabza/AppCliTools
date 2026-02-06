using System;
using System.IO;
using AppCliTools.DbContextAnalyzer.Domain;
using AppCliTools.DbContextAnalyzer.Models;
using BackendCarcass.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared;

namespace AppCliTools.DbContextAnalyzer;

public sealed class SeederCodeCreatorStarter
{
    private readonly ILogger _logger;
    private readonly CreateProjectSeederCodeParameters _par;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SeederCodeCreatorStarter(ILogger logger, CreateProjectSeederCodeParameters par)
    {
        _logger = logger;
        _par = par;
    }

    public void Go(DbContext contextDbSc, DbContext devContext)
    {
        string pathToContentRoot = Directory.GetCurrentDirectory();
        Console.WriteLine("pathToContentRoot=" + pathToContentRoot);

        var excludesRulesParameters =
            ExcludesRulesParametersDomain.CreateInstance(_par.ExcludesRulesParametersFilePath);

        var carcassOptionsBuilder = new DbContextOptionsBuilder<CarcassDbContext>();

        if (string.IsNullOrWhiteSpace(_par.ConnectionStringProd))
        {
            StShared.WriteErrorLine("ConnectionStringProd is empty", true);
            return;
        }

        //if (string.IsNullOrWhiteSpace(_par.ConnectionStringDev))
        //{
        //    StShared.WriteErrorLine("ConnectionStringDev is empty", true);
        //    return;
        //}

        if (string.IsNullOrWhiteSpace(_par.ProjectPrefix))
        {
            StShared.WriteErrorLine("ProjectPrefix is empty", true);
            return;
        }

        if (string.IsNullOrWhiteSpace(_par.GetJsonProjectPlacePath))
        {
            StShared.WriteErrorLine("GetJsonProjectPlacePath is empty", true);
            return;
        }

        if (string.IsNullOrWhiteSpace(_par.GetJsonProjectNamespace))
        {
            StShared.WriteErrorLine("GetJsonProjectNamespace is empty", true);
            return;
        }

        if (string.IsNullOrWhiteSpace(_par.ProjectPrefixShort))
        {
            StShared.WriteErrorLine("ProjectPrefixShort is empty", true);
            return;
        }

        if (string.IsNullOrWhiteSpace(_par.DataSeedingProjectPlacePath))
        {
            StShared.WriteErrorLine("DataSeedingProjectPlacePath is empty", true);
            return;
        }

        if (string.IsNullOrWhiteSpace(_par.DataSeedingProjectNamespace))
        {
            StShared.WriteErrorLine("DataSeedingProjectNamespace is empty", true);
            return;
        }

        if (string.IsNullOrWhiteSpace(_par.DataSeedingProjectSolutionFolder))
        {
            StShared.WriteErrorLine("DataSeedingProjectSolutionFolder is empty", true);
            return;
        }

        if (string.IsNullOrWhiteSpace(_par.MainDatabaseProjectName))
        {
            StShared.WriteErrorLine("MainDatabaseProjectName is empty", true);
            return;
        }

        if (string.IsNullOrWhiteSpace(_par.ProjectDbContextClassName))
        {
            StShared.WriteErrorLine("ProjectDbContextClassName is empty", true);
            return;
        }

        string modelFullPath =
            Path.Combine(_par.DataSeedingProjectPlacePath, _par.DataSeedingProjectNamespace, "Models");
        if (FileStat.CreateFolderIfNotExists(modelFullPath, true) is null)
        {
            StShared.WriteErrorLine("modelFullPath does not created", true);
            return;
        }

        carcassOptionsBuilder.UseSqlServer(_par.ConnectionStringProd);
        // ReSharper disable once using
        // ReSharper disable once DisposableConstructor
        using var carcassContext = new CarcassDbContext(carcassOptionsBuilder.Options);

        var getJsonCreatorParameters = new GetJsonCreatorParameters(
            _par.ProjectPrefix.Replace('.', '_') + "DbScContext", _par.ProjectPrefix + "ScaffoldSeederDbSc", "Models",
            _par.GetJsonProjectPlacePath, _par.GetJsonProjectNamespace);

        var seederCodeCreatorParameters = new SeederCodeCreatorParameters(_par.ProjectPrefix, _par.ProjectPrefixShort,
            "Models", _par.ProjectPrefix + "Seeders", "CarcassSeeders", _par.DataSeedingProjectPlacePath,
            _par.DataSeedingProjectNamespace, $"I{_par.ProjectPrefixShort}DataSeederRepository",
            $"I{_par.ProjectPrefixShort}KeyNameIdRepository", _par.ProjectPrefixShort + "DataSeedersFactory",
            _par.MainDatabaseProjectName, _par.ProjectDbContextClassName, "Models",
            _par.ProjectPrefixShort + "DataSeeder");

        var dataExtractor = new SeederCodeCreator(_logger, carcassContext, contextDbSc, devContext,
            getJsonCreatorParameters, seederCodeCreatorParameters, excludesRulesParameters);
        dataExtractor.Go();
    }
}
