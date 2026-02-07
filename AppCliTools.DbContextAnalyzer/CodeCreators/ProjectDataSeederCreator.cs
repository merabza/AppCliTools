using System;
using AppCliTools.CodeTools;
using AppCliTools.DbContextAnalyzer.Domain;
using AppCliTools.DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared;

namespace AppCliTools.DbContextAnalyzer.CodeCreators;

public sealed class ProjectDataSeederCreator : SeederCodeCreatorBase
{
    private readonly SeederCodeCreatorParameters _parameters;

    private int _counter;
    private CodeBlock? _seedProjectSpecificDataMethodCodeBlock;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ProjectDataSeederCreator(ILogger logger, SeederCodeCreatorParameters parameters,
        ExcludesRulesParametersDomain excludesRulesParameters) : base(logger, excludesRulesParameters,
        parameters.PlacePath, "ProjectDataSeeder.cs")
    {
        _parameters = parameters;
        _counter = 0;
    }

    public override void CreateFileStructure()
    {
        _seedProjectSpecificDataMethodCodeBlock = new CodeBlock("protected virtual bool SeedProjectData()",
            $"var seederFactory = ({_parameters.ProjectDataSeedersFactoryClassName}) DataSeedersFactory", string.Empty,
            "Logger.LogInformation(\"Seed Project Data Started\")");

        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using System", "using BackendCarcass.DataSeeding", "using Microsoft.Extensions.Logging",
            $"namespace {_parameters.ProjectNamespace}", string.Empty,
            new CodeBlock("public /*open*/ class ProjectDataSeeder : CarcassDataSeeder",
                new CodeBlock(
                    "protected ProjectDataSeeder(ILogger logger, CarcassDataSeedersFactory dataSeedersFactory, bool checkOnly) : base(logger, dataSeedersFactory, checkOnly)"),
                new CodeBlock("public override bool SeedData()", "return base.SeedData() && SeedProjectData()"),
                _seedProjectSpecificDataMethodCodeBlock));
        CodeFile.AddRange(block.CodeItems);
    }

    public void UseEntity(EntityData entityData)
    {
        string tableName = GetNewTableName(entityData.TableName);
        string tableNameCapitalCamel = tableName.CapitalizeCamel();

        _counter++;

        var block = new CodeBlock(string.Empty, string.Empty,
            $"Logger.LogInformation(\"Seeding {tableNameCapitalCamel}\")", string.Empty,
            new OneLineComment($"{_counter} {tableNameCapitalCamel}"),
            new CodeBlock($"if (!Use(seederFactory.Create{tableNameCapitalCamel}Seeder()))", "return false"));

        if (_seedProjectSpecificDataMethodCodeBlock is null)
        {
            throw new Exception("_seedProjectSpecificDataMethodCodeBlock is null");
        }

        _seedProjectSpecificDataMethodCodeBlock.AddRange(block.CodeItems);
    }

    public override void FinishAndSave()
    {
        var block = new CodeBlock(string.Empty, string.Empty, "Console.WriteLine(\"DataSeederCreator.Run Finished\")",
            "return true");

        if (_seedProjectSpecificDataMethodCodeBlock is null)
        {
            throw new Exception("_seedProjectSpecificDataMethodCodeBlock is null");
        }

        _seedProjectSpecificDataMethodCodeBlock.AddRange(block.CodeItems);
        base.FinishAndSave();
    }
}
