﻿using LibDataInput;
using LibParameters;

namespace CliParameters.FieldEditors;

public /*open*/ class TextFieldEditor : FieldEditor<string>
{
    private readonly string? _defaultValue;
    private readonly char _passwordCharacter;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TextFieldEditor(string propertyName, string? defaultValue = null, char passwordCharacter = '\0') : base(
        propertyName, true)
    {
        _defaultValue = defaultValue;
        _passwordCharacter = passwordCharacter;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        var curValue = GetValue(recordForUpdate, _defaultValue);
        //var curValue = GetValue(recordForUpdate, true, _defaultValue);//20220811

        SetValue(recordForUpdate, Inputer.InputText(FieldName, curValue, _passwordCharacter));
    }


    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record) ?? string.Empty;
        return _passwordCharacter == 0 || val == string.Empty ? val : new string(_passwordCharacter, val.Length);

        //სტანდარტული ფორმატის გადაყვანა custom ფორმატში
        //DateTime.Now.ToString("G", CultureInfo.InvariantCulture);
        //var v = DateTime.Now.ToString("G", CultureInfo.InvariantCulture);
        //ამ მასივი პირველი ელემენტი ემთხვევა სტანდარტულ ფორმატს. დანარჩენები ალბათ გამოიყენება სტრიქონის გაპარსვისას
    }

    public override void SetDefault(ItemData currentItem)
    {
        SetValue(currentItem, _defaultValue);
    }
}