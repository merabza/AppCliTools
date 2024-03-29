﻿using LibDataInput;
using LibParameters;

namespace CliParameters.FieldEditors;

public sealed class BoolFieldEditor : FieldEditor<bool>
{
    private readonly bool _defaultValue;

    // ReSharper disable once ConvertToPrimaryConstructor
    public BoolFieldEditor(string propertyName, bool defaultValue) : base(propertyName, true)
    {
        _defaultValue = defaultValue;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        SetValue(recordForUpdate, Inputer.InputBool(FieldName, GetValue(recordForUpdate, _defaultValue)));
    }

    public override void SetDefault(ItemData currentItem)
    {
        SetValue(currentItem, _defaultValue);
    }
}