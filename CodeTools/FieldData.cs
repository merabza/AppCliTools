using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CodeTools;

public sealed class FieldData
{
    // ReSharper disable once ConvertToPrimaryConstructor
    private FieldData(string name, string oldName, string realTypeName, string fullName, bool isNullable,
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

    public static FieldData Create(Type? tableClrType, IProperty s, string preferredName, FieldData? parent)
    {
        var clrType = s.ClrType;
        var isNullable = clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>);
        if (isNullable)
            clrType = clrType.GetGenericArguments()[0];
        else
            isNullable = IsAdvanceNullable(tableClrType, s.Name);

        var isNullableByParents = parent == null ? isNullable : parent.IsNullableByParents || isNullable;
        var realTypeName = GetRealTypeName(clrType.Name, s.GetColumnType(), isNullable, isNullableByParents);
        return new FieldData(preferredName, s.Name, realTypeName,
            (parent == null ? string.Empty : parent.FullName) + preferredName, isNullable, isNullableByParents);
    }

    private static bool IsAdvanceNullable(Type? clrType, string fieldName)
    {
        if (clrType == null)
            return false;

        var clrProperties = clrType.GetProperties();
        //foreach (var fieldData in entityData.FieldsData)
        //{
        //    if (fieldData.IsNullable)
        //        continue;

        var prop = clrProperties.SingleOrDefault(x => x.Name == fieldName);

        var attr = prop?.CustomAttributes.SingleOrDefault(x => x.AttributeType.Name == "NullableAttribute");

        attr = attr ??
               prop?.PropertyType.CustomAttributes.SingleOrDefault(x => x.AttributeType.Name == "NullableAttribute");

        if (attr is null)
            return false;

        if (attr.ConstructorArguments.Count < 1)
            return false;

        if (attr.ConstructorArguments[0].Value is not byte)
            return false;

        var b = (byte)(attr.ConstructorArguments[0].Value ?? 0);

        return b == 2;
    }

    private static string GetRealTypeName(string clrTypeName, string typeName, bool isNullable,
        bool isNullableByParents)
    {
        var realTypeCandidate = clrTypeName switch
        {
            "Int32" => "int",
            "String" => "string",
            "Byte[]" => "byte[]",
            "Boolean" => "bool",
            "Single" => "float",
            "Double" => "double",
            "Decimal" => "decimal",
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

        return $"{realTypeCandidate}{(isNullable || isNullableByParents ? "?" : string.Empty)}";
    }
}