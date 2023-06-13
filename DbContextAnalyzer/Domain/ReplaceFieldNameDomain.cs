namespace DbContextAnalyzer.Domain;

public sealed class ReplaceFieldNameDomain
{
    public ReplaceFieldNameDomain(string tableName, string oldFieldName, string newFieldName)
    {
        TableName = tableName;
        OldFieldName = oldFieldName;
        NewFieldName = newFieldName;
    }

    public string TableName { get; set; }
    public string OldFieldName { get; set; }
    public string NewFieldName { get; set; }
}