using System.Linq;

namespace CodeTools;

public sealed class FieldData
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public FieldData(string name, string oldName, string realTypeName, string fullName, bool isNullable,
        bool isNullableByParents)
    {
        Name = name;
        OldName = oldName;
        RealTypeName = realTypeName;
        FullName = fullName;
        IsNullable = isNullable;
        IsNullableByParents = isNullableByParents;
    }

    public string Name { get; set; }
    public string FullName { get; set; }
    public SubstituteFieldData? SubstituteField { get; set; }
    public string RealTypeName { get; set; }
    public bool IsNullable { get; set; }
    public bool IsNullableByParents { get; }
    public string? NavigationFieldName { get; set; }
    public string OldName { get; set; }

    public int GetLevel()
    {
        if (SubstituteField == null)
            return 0;
        return SubstituteField.Fields.Select(m => m.GetLevel()).DefaultIfEmpty().Max() + 1;
    }
}