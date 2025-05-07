﻿using System.Collections.Generic;

// ReSharper disable CollectionNeverUpdated.Global

namespace DbContextAnalyzer.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ExcludesRulesParameters
{
    public List<string> ExcludeTables { get; } = [];
    public List<SingularityException> SingularityExceptions { get; } = [];
    public List<TableFieldModel> ExcludeFields { get; } = [];
    public List<ReplaceFieldName> ReplaceFieldNames { get; } = [];
    public List<ReplaceTableName> ReplaceTableNames { get; } = [];
    public List<KeyFieldNamesModel> KeyFieldNames { get; } = [];
}