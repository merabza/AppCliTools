using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CodeTools;

public sealed class FieldData
{
    // ReSharper disable once ConvertToPrimaryConstructor
    //private FieldData(string name, string oldName, string realTypeName, string fullName, bool isNullable,
    //    bool isNullableByParents)
    //{
    //    Name = name;
    //    OldName = oldName;
    //    RealTypeName = realTypeName;
    //    FullName = fullName;
    //    IsNullable = isNullable;
    //    IsNullableByParents = isNullableByParents;
    //}

    public required string Name { get; set; }
    public required string FullName { get; set; }
    public SubstituteFieldData? SubstituteField { get; set; }
    public required string RealTypeName { get; set; }
    public bool IsNullable { get; set; }
    public bool IsNullableByParents { get; set; }
    public bool IsValueType { get; set; }
    public string? NavigationFieldName { get; set; }
    public required string OldName { get; set; }

    public int GetLevel()
    {
        if (SubstituteField == null)
        {
            return 0;
        }

        return SubstituteField.Fields.Select(m => m.GetLevel()).DefaultIfEmpty().Max() + 1;
    }

    public static FieldData Create(Type? tableClrType, IProperty s, string preferredName, FieldData? parent)
    {
        Type clrType = s.ClrType;
        bool isNullable = clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>);
        if (isNullable)
        {
            clrType = clrType.GetGenericArguments()[0];
        }
        else
        {
            isNullable = IsAdvanceNullable(tableClrType, s.Name);
        }

        bool isNullableByParents = parent == null ? isNullable : parent.IsNullableByParents || isNullable;
        string realTypeName = GetRealTypeName(clrType.Name, s.GetColumnType(), isNullable, isNullableByParents);

        return new FieldData
        {
            Name = preferredName,
            OldName = s.Name,
            RealTypeName = realTypeName,
            FullName = (parent?.FullName ?? string.Empty) + preferredName,
            IsNullable = isNullable,
            IsNullableByParents = isNullableByParents,
            IsValueType = clrType.IsValueType
        };
    }

    private static bool IsAdvanceNullable(Type? clrType, string fieldName)
    {
        if (clrType == null)
        {
            return false;
        }

        PropertyInfo[] clrProperties = clrType.GetProperties();

        PropertyInfo? prop = clrProperties.SingleOrDefault(x => x.Name == fieldName);

        CustomAttributeData? attr =
            prop?.CustomAttributes.SingleOrDefault(x => x.AttributeType.Name == "NullableAttribute");

        attr ??= prop?.PropertyType.CustomAttributes.SingleOrDefault(x => x.AttributeType.Name == "NullableAttribute");

        if (attr is null)
        {
            return false;
        }

        if (attr.ConstructorArguments.Count < 1)
        {
            return false;
        }

        if (attr.ConstructorArguments[0].Value is not byte)
        {
            return false;
        }

        byte b = (byte)(attr.ConstructorArguments[0].Value ?? 0);

        return b == 2;
    }

    private static string GetRealTypeName(string clrTypeName, string typeName, bool isNullable,
        bool isNullableByParents)
    {
        string realTypeCandidate = clrTypeName switch
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
