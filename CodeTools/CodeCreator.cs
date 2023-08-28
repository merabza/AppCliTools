using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace CodeTools;

public /*open*/ class CodeCreator
{
    private readonly ILogger _logger;
    private readonly string _placePath;
    protected readonly CodeFile CodeFile;

    public CodeCreator(ILogger logger, string placePath, string? codeFileName = null)
    {
        _logger = logger;
        _placePath = placePath;
        CodeFile = new CodeFile(codeFileName);
        _logger.LogInformation("create Code file started -> {codeFileName}", codeFileName);
    }

    public virtual void CreateFileStructure()
    {
    }

    public virtual void UseEntity(EntityData entityData, bool isCarcassType)
    {
    }

    public virtual void UseEntity(IEntityType entityType, bool isCarcassType, List<string> ignoreFields)
    {
    }

    public virtual void FinishAndSave()
    {
        CreateFile();
    }

    protected static string GetRealTypeName(string clrTypeName, string typeName, bool isNullable)
    {
        var realTypeCandidate = clrTypeName switch
        {
            "Int32" => "int",
            "String" => "string",
            "Byte[]" => "byte[]",
            "Boolean" => "bool",
            "Int16" => "short",
            _ => null
        };

        if (realTypeCandidate != null)
            return realTypeCandidate;

        return typeName switch
        {
            "smallint" => $"short{(isNullable ? "?" : "")}",
            "int" => $"int{(isNullable ? "?" : "")}",
            "bit" => $"bool{(isNullable ? "?" : "")}",
            _ => typeName
        };
    }

    protected void CreateFile(string? codePath = null)
    {
        var strCode = CodeFile.Output(-1);
        var placePath = codePath ?? _placePath;
        StShared.CreateFolder(placePath, true);
        if (CodeFile.FileName is null)
            throw new Exception("CodeFile.FileName is null");
        var forCreateFileName = Path.Combine(placePath, CodeFile.FileName);
        File.WriteAllText(forCreateFileName, strCode);
        _logger.LogInformation("Code file created: {forCreateFileName}", forCreateFileName);
    }

    private static string GetTableNameSingular(IReadOnlyDictionary<string, string> singularityExceptions,
        string tableName)
    {
        if (singularityExceptions.TryGetValue(tableName, out var singular))
            return singular;
        var unCapTableName = tableName.UnCapitalize();
        return singularityExceptions.TryGetValue(unCapTableName, out var exception)
            ? exception
            : tableName.Singularize();
    }

    protected static string GetTableNameSingularCapitalizeCamel(Dictionary<string, string> singularityExceptions,
        string tableName)
    {
        return GetTableNameSingular(singularityExceptions, tableName).CapitalizeCamel();
    }

    protected static (bool, List<IProperty>) GetFieldsProperties(IEntityType entityType, List<string> ignoreFields)
    {
        var props = entityType.GetProperties()
            .Where(p => p.ValueGenerated == ValueGenerated.Never && !ignoreFields.Contains(p.Name)).ToList();

        if (props.Count > 0)
            return (false, props);

        //თუ ცხრილის ველები ყველა ავტომატურად გენერირდება, მაშინ ავიღოთ მხოლოდ ძირითადი გასაღების ველები და ის გავიტანოთ json-ში
        //და ის დაგვჭირდება SeederModel-ში
        props = entityType.GetKeys().SelectMany(s => s.Properties).ToList();
        if (props.Count != 1) throw new Exception("გასაღები ზუსტად ერთი უნდა იყოს. სხვა ვარიანტები ჯერ არ განიხილება");

        return (true, props);
    }
}