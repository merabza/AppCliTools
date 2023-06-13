using LibDataInput;
using LibParameters;

namespace CliParameters.FieldEditors;

public sealed class OptionalTextFieldEditor : FieldEditor<string?>
{
    private readonly string? _defaultValue;
    private readonly char _passwordCharacter;

    public OptionalTextFieldEditor(string propertyName, string? propertyDescriptor = null,
        string? defaultValue = default, char passwordCharacter = default) : base(propertyName, propertyDescriptor)
    {
        _defaultValue = defaultValue;
        _passwordCharacter = passwordCharacter;
    }

    public override void UpdateField(string? recordName, object recordForUpdate) //, object currentRecord
    {
        var curValue = GetValue(recordForUpdate, _defaultValue);
        //var curValue = GetValue(recordForUpdate, true, _defaultValue);//20220811

        SetValue(recordForUpdate, Inputer.InputText(FieldName, curValue, _passwordCharacter));
    }


    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record) ?? "";
        return _passwordCharacter == default || val == "" ? val : new string(_passwordCharacter, val.Length);

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