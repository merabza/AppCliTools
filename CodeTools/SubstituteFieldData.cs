using System.Collections.Generic;

namespace CodeTools;

public sealed class SubstituteFieldData
{
    //public SubstituteFieldData(FieldData parent, string tableName, string primaryKeyFieldName, List<FieldData> fields)
    //{
    //    //Parent = parent;
    //    TableName = tableName;
    //    //PrimaryKeyFieldName = primaryKeyFieldName;
    //    Fields = fields;
    //}

    public SubstituteFieldData(string tableName, List<FieldData> fields)
    {
        TableName = tableName;
        Fields = fields;
    }

    public string TableName { get; set; }

    public List<FieldData> Fields { get; set; }
    //[JsonIgnore]
    //public FieldData Parent { get; set; }
    //public string PrimaryKeyFieldName { get; set; }
}