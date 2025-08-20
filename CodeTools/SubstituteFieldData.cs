using System.Collections.Generic;

namespace CodeTools;

public sealed class SubstituteFieldData
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SubstituteFieldData(string tableName, List<FieldData> fields)
    {
        TableName = tableName;
        Fields = fields;
    }

    //public bool UseTempData { get; set; }
    public string TableName { get; set; }

    public List<FieldData> Fields { get; set; }
}