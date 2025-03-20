﻿using LibDataInput;

namespace CliParameters.FieldEditors;

public sealed class OptionalTextFieldEditor : FieldEditor<string?>
{
    private readonly string? _defaultValue;
    private readonly char _passwordCharacter;

    // ReSharper disable once ConvertToPrimaryConstructor
    public OptionalTextFieldEditor(string propertyName, bool enterFieldDataOnCreate = false,
        string? propertyDescriptor = null, string? defaultValue = null, char passwordCharacter = '\0') : base(
        propertyName, enterFieldDataOnCreate, null, false, propertyDescriptor)
    {
        _defaultValue = defaultValue;
        _passwordCharacter = passwordCharacter;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        var curValue = GetValue(recordForUpdate, _defaultValue);
        SetValue(recordForUpdate, Inputer.InputText(FieldName, curValue, _passwordCharacter));
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record) ?? string.Empty;
        return _passwordCharacter == 0 || val == string.Empty ? val : new string(_passwordCharacter, val.Length);
    }
}