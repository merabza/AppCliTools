﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DbContextAnalyzer.Models;
using SystemToolsShared;

namespace DbContextAnalyzer.Domain;

public sealed class ExcludesRulesParametersDomain
{
    public List<string> ExcludeTables { get; } = [];
    public required ImmutableSortedDictionary<string, string> SingularityExceptions { get; set; }
    public List<TableFieldDomain> ExcludeFields { get; } = [];
    public List<ReplaceFieldNameDomain> ReplaceFieldNames { get; } = [];
    public required ImmutableSortedDictionary<string, string> ReplaceTableNames { get; set; }
    public List<KeyFieldNamesDomain> KeyFieldNames { get; } = [];

    public static ExcludesRulesParametersDomain CreateInstance(string? excludesRulesParametersFilePath)
    {
        ExcludesRulesParameters? excludesRulesParameters = null;
        if (!string.IsNullOrWhiteSpace(excludesRulesParametersFilePath))
            excludesRulesParameters =
                FileLoader.LoadDeserializeResolve<ExcludesRulesParameters>(excludesRulesParametersFilePath, true);

        var se = excludesRulesParameters?.SingularityExceptions
            .Where(rfn =>
                !string.IsNullOrWhiteSpace(rfn.TableName) && !string.IsNullOrWhiteSpace(rfn.TableNameSingular))
            .ToDictionary(rfn => rfn.TableName!, rfn => rfn.TableNameSingular!) ?? [];

        var rtn = excludesRulesParameters?.ReplaceTableNames
            .Where(rfn => !string.IsNullOrWhiteSpace(rfn.OldTableName) && !string.IsNullOrWhiteSpace(rfn.NewTableName))
            .ToDictionary(rfn => rfn.OldTableName!, rfn => rfn.NewTableName!) ?? [];

        var exclRulParDom = new ExcludesRulesParametersDomain
        {
            SingularityExceptions = se.ToImmutableSortedDictionary(),
            ReplaceTableNames = rtn.ToImmutableSortedDictionary()
        };

        if (excludesRulesParameters is null)
            return exclRulParDom;

        foreach (var excludeTable in excludesRulesParameters.ExcludeTables.Where(x => !string.IsNullOrWhiteSpace(x)))
            exclRulParDom.ExcludeTables.Add(excludeTable);

        foreach (var tfm in excludesRulesParameters.ExcludeFields.Where(tfm =>
                     !string.IsNullOrWhiteSpace(tfm.TableName) && !string.IsNullOrWhiteSpace(tfm.FieldName)))
            exclRulParDom.ExcludeFields.Add(new TableFieldDomain
            {
                TableName = tfm.TableName!, FieldName = tfm.FieldName!
            });

        foreach (var rfn in excludesRulesParameters.ReplaceFieldNames.Where(rfn =>
                     !string.IsNullOrWhiteSpace(rfn.TableName) && !string.IsNullOrWhiteSpace(rfn.OldFieldName) &&
                     !string.IsNullOrWhiteSpace(rfn.NewFieldName)))
            exclRulParDom.ReplaceFieldNames.Add(new ReplaceFieldNameDomain
            {
                TableName = rfn.TableName!, OldFieldName = rfn.OldFieldName!, NewFieldName = rfn.NewFieldName!
            });

        foreach (var kfn in excludesRulesParameters.KeyFieldNames)
        {
            if (string.IsNullOrWhiteSpace(kfn.TableName))
                continue;
            var keys = kfn.Keys.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            if (keys.Count == 0)
                continue;
            exclRulParDom.KeyFieldNames.Add(new KeyFieldNamesDomain { TableName = kfn.TableName, Keys = keys });
        }

        return exclRulParDom;
    }

    public Dictionary<string, string> GetReplaceFieldsDictByTableName(string tableName)
    {
        var newTableName = GetReplaceTablesName(tableName);
        return ReplaceFieldNames.Where(w => w.TableName == newTableName)
            .ToDictionary(k => k.OldFieldName, v => v.NewFieldName);
    }

    public string GetReplaceTablesName(string tableName)
    {
        return ReplaceTableNames.GetValueOrDefault(tableName, tableName);
    }

    public string GetNewFieldName(string tableName, string oldFieldName)
    {
        var repField =
            ReplaceFieldNames.SingleOrDefault(x => x.TableName == tableName && x.OldFieldName == oldFieldName);
        return repField is null ? oldFieldName : repField.NewFieldName;
    }
}