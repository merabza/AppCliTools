using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeTools;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace DbContextAnalyzer.CodeCreators;

public sealed class SeederModelCreator : CodeCreator
{
    private readonly string _modelsFolderName;
    private readonly string _projectNamespace;

    private readonly Dictionary<string, string> _singularityExceptions;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SeederModelCreator(ILogger logger, string placePath, string projectNamespace, string modelsFolderName,
        Dictionary<string, string> singularityExceptions) : base(logger, Path.Combine(placePath, modelsFolderName))
    {
        _projectNamespace = projectNamespace;
        _modelsFolderName = modelsFolderName;
        _singularityExceptions = singularityExceptions;
    }

    public override void CreateFileStructure()
    {
    }

    public override void UseEntity(EntityData entityData, bool isCarcassType)
    {
        var tableName = entityData.TableName;

        var tableNameSingular = GetTableNameSingularCapitalizeCamel(_singularityExceptions, tableName);
        var className = tableNameSingular + "SeederModel";

        var fieldDataList = entityData.GetFlatFieldData();

        var constructorParameters = string.Join(", ",
            fieldDataList.Select(fd => $"{fd.RealTypeName} {ValidateIdentifier(fd.FullName.UnCapitalize())}")
                .ToArray());

        var constructorCodeBlock = new CodeBlock($"public {className}({constructorParameters})");

        constructorCodeBlock.AddRange(fieldDataList.Select(fd =>
            new CodeCommand($"{fd.FullName} = {ValidateIdentifier(fd.FullName.UnCapitalize())}")));

        var classCodeBlock = new CodeBlock("public sealed class " + className, "",
            new OneLineComment(" ReSharper disable once ConvertToPrimaryConstructor"), constructorCodeBlock);

        var usingSystem =
            fieldDataList.Any(a => a.RealTypeName == "DateTime") ? new CodeCommand("using System") : null;

        classCodeBlock.AddRange(fieldDataList.Select(fd =>
            new CodeBlock($"public {fd.RealTypeName} {fd.FullName}", true, "get", "set")));

        var block = new CodeBlock("",
            new OneLineComment($"Created by {GetType().Name} at {DateTime.Now}"),
            usingSystem,
            $"namespace {_projectNamespace}.{_modelsFolderName}",
            "",
            classCodeBlock);

        CodeFile.FileName = className + ".cs";
        CodeFile.AddRange(block.CodeItems);

        CreateFile();
    }


    private static string ValidateIdentifier(string identifier)
    {
        var provider = CodeDomProvider.CreateProvider("C#");
        return provider.IsValidIdentifier(identifier) ? identifier : $"@{identifier}";
    }
}