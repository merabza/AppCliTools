﻿using LibMenuInput;
using LibParameters;

namespace CliParameters.FieldEditors;

public /*open*/ class FilePathFieldEditor : FieldEditor<string>
{
    private readonly string? _defaultValue;

    public FilePathFieldEditor(string propertyName, string? defaultValue = default) : base(propertyName)
    {
        _defaultValue = defaultValue;
    }

    public override void UpdateField(string? recordName, object recordForUpdate)
    {
        SetValue(recordForUpdate, MenuInputer.InputFilePath(FieldName, GetValue(recordForUpdate, _defaultValue)));
    }

    public override void SetDefault(ItemData currentItem)
    {
        SetValue(currentItem, _defaultValue);
    }
}