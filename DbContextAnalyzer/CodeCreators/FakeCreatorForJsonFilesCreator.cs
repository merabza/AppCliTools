using System;
using CodeTools;
using DbContextAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace DbContextAnalyzer.CodeCreators;

public sealed class FakeCreatorForJsonFilesCreator : CodeCreator
{
    private readonly CreatorCreatorParameters _par;

    // ReSharper disable once ConvertToPrimaryConstructor
    public FakeCreatorForJsonFilesCreator(ILogger logger, CreatorCreatorParameters par) : base(logger,
        par.GetJsonProjectPlacePath, "JsonFilesCreator.cs")
    {
        _par = par;
    }

    public override void CreateFileStructure()
    {
        var dbContextClassName = $"{_par.ProjectPrefix.Replace('.', '_')}DbScContext";
        var dbContextProjectName = $"{_par.ProjectPrefix}ScaffoldSeederDbSc";

        var block = new CodeBlock(string.Empty,
            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            $"using {dbContextProjectName}",
            $"namespace {_par.GetJsonProjectNamespace}",
            string.Empty,
            new CodeBlock("public sealed class JsonFilesCreator",
                new CodeBlock(
                    $"public JsonFilesCreator({dbContextClassName} context, string seederCreatorJsonFolderName)"),
                new CodeBlock("public bool Run()", "return true")));
        CodeFile.AddRange(block.CodeItems);
    }
}