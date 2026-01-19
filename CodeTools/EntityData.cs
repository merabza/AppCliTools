using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CodeTools;

public sealed class EntityData
{
    public readonly List<FieldData> SelfRecursiveFields = [];
    public required string PrimaryKeyFieldName { get; set; }
    public required string TableName { get; set; }
    public required IEntityType EntityType { get; set; }
    public int Level { get; set; }

    //[JsonIgnore] public IIndex? OptimalIndex { get; set; }
    //public List<string> OptimalIndexFields { get; set; } = [];

    //public List<FieldData> OptimalIndexFieldsData { get; set; } = [];
    public List<IProperty> OptimalIndexProperties { get; set; } = [];
    public List<FieldData> FieldsData { get; set; } = [];
    public bool NeedsToCreateTempData { get; set; }
    public bool UsePrimaryKey { get; set; }
    public bool HasAutoNumber { get; set; }
    public bool HasAutoNumberByOneToOnePrincipal { get; set; }
    public bool HasOneToOneReference { get; set; }

    public List<FieldData> GetFlatFieldData(List<FieldData>? fieldsData = null)
    {
        var flat = new List<FieldData>();
        foreach (FieldData fieldData in fieldsData ?? FieldsData)
        {
            if (fieldData.SubstituteField is null || fieldData.SubstituteField.Fields.Count == 0)
            {
                AddOneWithCheckOnDuplicates(flat, fieldData);
            }
            else
            {
                foreach (FieldData fd in GetFlatFieldData(fieldData.SubstituteField.Fields))
                {
                    AddOneWithCheckOnDuplicates(flat, fd);
                }
            }
        }

        return flat;
    }

    private static void AddOneWithCheckOnDuplicates(List<FieldData> flat, FieldData fieldData)
    {
        flat.Add(fieldData);
    }
}
