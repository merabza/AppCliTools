using System.Collections.Generic;

// ReSharper disable CollectionNeverUpdated.Global

namespace DbContextAnalyzer.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ExcludesRulesParameters
{
    public List<string> ExcludeTables { get; init; } = [];
    public Dictionary<string, string> SingularityExceptions { get; init; } = new();
    public List<TableFieldModel> ExcludeFields { get; init; } = [];
    public List<ReplaceFieldName> ReplaceFieldNames { get; init; } = [];
    public List<KeyFieldNamesModel> KeyFieldNames { get; } = [];
}