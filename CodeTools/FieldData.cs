using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

    public static FieldData Create(IProperty s, string preferredName, FieldData? parent)
    {
        var clrType = s.ClrType;
        var isNullable = clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>);
        if (isNullable) clrType = clrType.GetGenericArguments()[0];
        var isNullableByParents = parent == null ? isNullable : parent.IsNullableByParents || isNullable;
        var realTypeName = GetRealTypeName(clrType.Name, s.GetColumnType(), isNullable);
        return new FieldData(preferredName, s.Name, realTypeName,
            (parent == null ? string.Empty : parent.FullName) + preferredName, isNullable, isNullableByParents);
    }

    private static string GetRealTypeName(string clrTypeName, string typeName, bool isNullable)
    {
        var realTypeCandidate = clrTypeName switch
        {
            "Int32" => "int",
            "String" => "string",
            "Byte[]" => "byte[]",
            "Boolean" => "bool",
            "Float" => "float",
            "Int16" => "short",
            "DateTime" => "DateTime",
            _ => null
        } ?? typeName switch
        {
            "smallint" => "short",
            "int" => "int",
            "bit" => "bool",
            "datetime2" => "DateTime",
            "date" => "DateTime",
            _ => typeName
        };

        return $"{realTypeCandidate}{(isNullable ? "?" : string.Empty)}";
    }
}