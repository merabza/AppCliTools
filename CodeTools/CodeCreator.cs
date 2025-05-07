using System;
using System.IO;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace CodeTools;

public /*open*/ class CodeCreator
{
    private readonly ILogger _logger;
    private readonly string _placePath;
    protected readonly CodeFile CodeFile;

    protected CodeCreator(ILogger logger, string placePath, string? codeFileName = null)
    {
        _logger = logger;
        _placePath = placePath;
        CodeFile = new CodeFile(codeFileName);
        _logger.LogInformation("create Code file started -> {codeFileName}", codeFileName);
    }

    public virtual void CreateFileStructure()
    {
    }

    //public virtual void UseEntity(EntityData entityData, bool isCarcassType)
    //{
    //}

    //public virtual void UseEntity(IEntityType entityType, bool isCarcassType, List<string> ignoreFields)
    //{
    //}

    public virtual void FinishAndSave()
    {
        CreateFile();
    }

    //ეს კოდი დავაკომენტარე, მაგრამ არ ვიცი სხვაგან საჭიროათუ არა
    //protected static string GetRealTypeName(string clrTypeName, string typeName, bool isNullable)
    //{
    //    var realTypeCandidate = clrTypeName switch
    //    {
    //        "Int32" => "int",
    //        "String" => "string",
    //        "Byte[]" => "byte[]",
    //        "Boolean" => "bool",
    //        "Int16" => "short",
    //        _ => null
    //    };

    //    if (realTypeCandidate != null)
    //        return realTypeCandidate;

    //    return typeName switch
    //    {
    //        "smallint" => $"short{(isNullable ? "?" : string.Empty)}",
    //        "int" => $"int{(isNullable ? "?" : string.Empty)}",
    //        "bit" => $"bool{(isNullable ? "?" : string.Empty)}",
    //        _ => typeName
    //    };
    //}

    private void CreateFile(string? codePath = null)
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
}