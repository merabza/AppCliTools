using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using CodeTools;
using DbContextAnalyzer.Domain;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace DbContextAnalyzer.CodeCreators;

public sealed class SeederModelCreator : SeederCodeCreatorBase
{
    private readonly string _modelsFolderName;
    private readonly string _projectNamespace;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SeederModelCreator(ILogger logger, string placePath, string projectNamespace, string modelsFolderName,
        ExcludesRulesParametersDomain excludesRulesParameters) : base(logger, excludesRulesParameters,
        Path.Combine(placePath, modelsFolderName))
    {
        _projectNamespace = projectNamespace;
        _modelsFolderName = modelsFolderName;
    }

    public override void CreateFileStructure()
    {
    }

    public void UseEntity(EntityData entityData)
    {
        var tableName = GetTableName(entityData.TableName);

        var tableNameSingular = GetTableNameSingularCapitalizeCamel(tableName);
        var className = tableNameSingular + "SeederModel";

        var fieldDataList = entityData.GetFlatFieldData();

        var constructorParameters = string.Join(", ",
            fieldDataList.Select(fd => $"{fd.RealTypeName} {ValidateIdentifier(fd.FullName.UnCapitalize())}")
                .ToArray());

        var constructorCodeBlock = new CodeBlock($"public {className}({constructorParameters})");

        constructorCodeBlock.AddRange(fieldDataList.Select(fd =>
            new CodeCommand($"{fd.FullName} = {ValidateIdentifier(fd.FullName.UnCapitalize())}")));

        var classCodeBlock = new CodeBlock("public sealed class " + className, string.Empty,
            new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"), constructorCodeBlock);

        var usingSystem = fieldDataList.Any(a => a.RealTypeName is "DateTime" or "DateTime?")
            ? new CodeCommand("using System")
            : null;

        classCodeBlock.AddRange(fieldDataList.Select(fd =>
            new CodeBlock($"public {fd.RealTypeName} {fd.FullName}", true, "get", "set")));

        var block = new CodeBlock(string.Empty, new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            usingSystem, $"namespace {_projectNamespace}.{_modelsFolderName}", string.Empty, classCodeBlock);

        CodeFile.FileName = className + ".cs";
        CodeFile.AddRange(block.CodeItems);

        FinishAndSave();
    }

    private static string ValidateIdentifier(string identifier)
    {
        // ReSharper disable once using
        using var provider = CodeDomProvider.CreateProvider("C#");
        return provider.IsValidIdentifier(identifier) ? identifier : $"@{identifier}";
    }
}