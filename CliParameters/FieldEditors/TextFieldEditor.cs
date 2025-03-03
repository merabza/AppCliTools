using LibDataInput;

namespace CliParameters.FieldEditors;

public /*open*/ class TextFieldEditor : FieldEditor<string>
{
    private readonly char _passwordCharacter;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TextFieldEditor(string propertyName, string? defaultValue = null, bool autoUsageOfDefaultValue = false,
        char passwordCharacter = '\0') : base(propertyName, true, defaultValue, autoUsageOfDefaultValue)
    {
        _passwordCharacter = passwordCharacter;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        var curValue = GetValue(recordForUpdate, DefaultValue);
        SetValue(recordForUpdate, Inputer.InputText(FieldName, curValue, _passwordCharacter));
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValueOrDefault(record);
        if (val is null || _passwordCharacter == 0 || val == string.Empty)
            return val ?? string.Empty;
        return new string(_passwordCharacter, val.Length);
    }

    //public override void SetDefault(ItemData currentItem)
    //{
    //    SetValue(currentItem, _defaultValue);
    //}
}