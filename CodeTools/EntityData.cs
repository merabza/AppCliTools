using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CodeTools;

public sealed class EntityData
{
    public FieldData? SelfRecursiveField;

    //IEntityType entityType, 
    public EntityData(string tableName)
    {
        //EntityType = entityType;
        TableName = tableName;
    }

    public string? PrimaryKeyFieldName { get; set; }

    [JsonIgnore]
    //public IEntityType EntityType { get; set; }
    public string TableName { get; }

    public int Level { get; set; }

    [JsonIgnore] public IIndex? OptimalIndex { get; set; }

    public List<FieldData> OptimalIndexFieldsData { get; set; } = new();
    public List<FieldData> FieldsData { get; set; } = new();
    public bool NeedsToCreateTempData { get; set; }
    public bool UsePrimaryKey { get; set; }

    public List<FieldData> GetFlatFieldData(List<FieldData>? fieldsData = null)
    {
        var flat = new List<FieldData>();
        foreach (var fieldData in fieldsData ?? FieldsData)
            if (fieldData.SubstituteField is null || fieldData.SubstituteField.Fields.Count == 0)
                //flat.Add(fieldData);
                AddOneWithCheckOnDuplicates(flat, fieldData);
            else
                //flat.AddRange(GetFlatFieldData(fieldData.SubstituteField.Fields));
                foreach (var fd in GetFlatFieldData(fieldData.SubstituteField.Fields))
                    AddOneWithCheckOnDuplicates(flat, fd);
        return flat;
    }

    private void AddOneWithCheckOnDuplicates(List<FieldData> flat, FieldData fieldData)
    {
        //List<FieldData> duplicates = flat.Where(w => w.Name == fieldData.Name).ToList();
        //if (duplicates.Count > 0)
        //{
        //    duplicates.ForEach(f => f.NeedsToUseTableName = true);
        //    fieldData.NeedsToUseTableName = true;
        //}
        flat.Add(fieldData);
    }
}