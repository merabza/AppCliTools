using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CodeTools;

public sealed class EntityData
{
    public FieldData? SelfRecursiveField;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EntityData(string tableName)
    {
        TableName = tableName;
    }

    public string? PrimaryKeyFieldName { get; set; }

    [JsonIgnore] public string TableName { get; }

    public int Level { get; set; }

    [JsonIgnore] public IIndex? OptimalIndex { get; set; }

    public List<FieldData> OptimalIndexFieldsData { get; set; } = [];
    public List<FieldData> FieldsData { get; set; } = [];
    public bool NeedsToCreateTempData { get; set; }
    public bool UsePrimaryKey { get; set; }

    public List<FieldData> GetFlatFieldData(List<FieldData>? fieldsData = null)
    {
        var flat = new List<FieldData>();
        foreach (var fieldData in fieldsData ?? FieldsData)
            if (fieldData.SubstituteField is null || fieldData.SubstituteField.Fields.Count == 0)
                AddOneWithCheckOnDuplicates(flat, fieldData);
            else
                foreach (var fd in GetFlatFieldData(fieldData.SubstituteField.Fields))
                    AddOneWithCheckOnDuplicates(flat, fd);
        return flat;
    }

    private static void AddOneWithCheckOnDuplicates(List<FieldData> flat, FieldData fieldData)
    {
        flat.Add(fieldData);
    }
}