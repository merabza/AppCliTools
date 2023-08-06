using System;
using LibMenuInput;
using LibParameters;

namespace CliParameters.FieldEditors;

public /*open*/ class EnumFieldEditor<TEnum> : FieldEditor<TEnum> where TEnum : struct, Enum
{
    private readonly TEnum _defaultValue;

    public EnumFieldEditor(string propertyName, TEnum defaultValue) : base(propertyName)
    {
        _defaultValue = defaultValue;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        var current = GetValue(recordForUpdate, _defaultValue);
        //var current = GetValue(recordForUpdate, true, _defaultValue);//20220811
        SetValue(recordForUpdate, MenuInputer.InputFromEnumList(FieldName, current));
    }

    public override void SetDefault(ItemData currentItem)
    {
        SetValue(currentItem, _defaultValue);
    }
}