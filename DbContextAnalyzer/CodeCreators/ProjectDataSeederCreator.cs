using System;
using CodeTools;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace DbContextAnalyzer.CodeCreators;

public sealed class ProjectDataSeederCreator : CodeCreator
{
    private readonly SeederCodeCreatorParameters _parameters;


    private int _counter;
    private CodeBlock? _seedProjectSpecificDataMethodCodeBlock;

    public ProjectDataSeederCreator(ILogger logger, SeederCodeCreatorParameters parameters) : base(logger,
        parameters.PlacePath, "ProjectDataSeeder.cs")
    {
        _parameters = parameters;
        _counter = 0;
    }

    public override void CreateFileStructure()
    {
        _seedProjectSpecificDataMethodCodeBlock = new CodeBlock(
            "protected override Option<Err[]> SeedProjectSpecificData()",
            $"{_parameters.ProjectDataSeedersFabricClassName} seederFabric = ({_parameters.ProjectDataSeedersFabricClassName}) DataSeedersFabric",
            "",
            "Logger.LogInformation(\"Seed Project Data Started\")"
        );

        var block = new CodeBlock("",
            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            "using System",
            "using CarcassDataSeeding",
            "using LanguageExt",
            "using Microsoft.Extensions.Logging",
            "using SystemToolsShared",
            $"namespace {_parameters.ProjectNamespace}",
            "",
            new CodeBlock("public /*open*/ class ProjectDataSeeder : CarcassDataSeeder",
                new CodeBlock(
                    "protected ProjectDataSeeder(ILogger<CarcassDataSeeder> logger, DataSeedersFabric dataSeedersFabric, bool checkOnly) : base(logger, dataSeedersFabric, checkOnly)"),
                _seedProjectSpecificDataMethodCodeBlock));
        CodeFile.AddRange(block.CodeItems);
    }

    public override void UseEntity(EntityData entityData, bool isCarcassType)
    {
        var tableName = entityData.TableName;
        var tableNameCapitalCamel = tableName.CapitalizeCamel();

        _counter++;

        var block = new CodeBlock("",
            "",
            $"Logger.LogInformation(\"Seeding {tableNameCapitalCamel}\")",
            "",
            new OneLineComment($"{_counter} {tableNameCapitalCamel}"),
            $"var result{_counter} = Use(seederFabric.Create{tableNameCapitalCamel}Seeder())",
            new CodeBlock($"if (result{_counter}.IsSome)", $"return (Err[])result{_counter}"));

        if (_seedProjectSpecificDataMethodCodeBlock is null)
            throw new Exception("_seedProjectSpecificDataMethodCodeBlock is null");

        _seedProjectSpecificDataMethodCodeBlock.AddRange(block.CodeItems);
    }

    public override void FinishAndSave()
    {
        var block = new CodeBlock("", "", "Console.WriteLine(\"DataSeederCreator.Run Finished\")", "return null");

        if (_seedProjectSpecificDataMethodCodeBlock is null)
            throw new Exception("_seedProjectSpecificDataMethodCodeBlock is null");

        _seedProjectSpecificDataMethodCodeBlock.AddRange(block.CodeItems);
        CreateFile();
    }
}