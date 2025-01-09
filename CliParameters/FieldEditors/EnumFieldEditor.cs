using System;
using LibMenuInput;
using LibParameters;

namespace CliParameters.FieldEditors;

public /*open*/ class EnumFieldEditor<TEnum> : FieldEditor<TEnum> where TEnum : struct, Enum
{
    private readonly TEnum _defaultValue;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EnumFieldEditor(string propertyName, TEnum defaultValue, bool enterFieldDataOnCreate = false) : base(
        propertyName, enterFieldDataOnCreate)
    {
        _defaultValue = defaultValue;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        var current = GetValue(recordForUpdate, _defaultValue);
        SetValue(recordForUpdate, MenuInputer.InputFromEnumList(FieldName, current));
    }

    public override void SetDefault(ItemData currentItem)
    {
        SetValue(currentItem, _defaultValue);
    }
}