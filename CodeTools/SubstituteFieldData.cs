using System.Collections.Generic;

namespace CodeTools;

public sealed class SubstituteFieldData
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SubstituteFieldData(string tableName, List<FieldData> fields, bool hasAutoNumber)
    {
        TableName = tableName;
        Fields = fields;
        HasAutoNumber = hasAutoNumber;
    }

    public bool HasAutoNumber { get; set; }
    public string TableName { get; set; }

    public List<FieldData> Fields { get; set; }
}