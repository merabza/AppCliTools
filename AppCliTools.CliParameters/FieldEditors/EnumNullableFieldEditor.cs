using System;
using AppCliTools.LibMenuInput;

namespace AppCliTools.CliParameters.FieldEditors;

public /*open*/ class EnumNullableFieldEditor<TEnum> : FieldEditor<TEnum?> where TEnum : struct, Enum
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public EnumNullableFieldEditor(string propertyName, TEnum defaultValue, bool autoUsageOfDefaultValue = false,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate, defaultValue,
        autoUsageOfDefaultValue)
    {
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        TEnum? current = GetValue(recordForUpdate, DefaultValue);
        SetValue(recordForUpdate, MenuInputer.InputFromEnumNullableList(FieldName, current));
    }

    public override string GetValueStatus(object? record)
    {
        TEnum? val = GetValueOrDefault(record);
        return val?.ToString() ?? string.Empty;
    }
}
