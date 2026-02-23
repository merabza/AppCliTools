using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;

namespace AppCliTools.CliParameters.FieldEditors;

public /*open*/ class TextFieldEditor : FieldEditor<string>
{
    private readonly char _passwordCharacter;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TextFieldEditor(string propertyName, string? defaultValue = null, bool autoUsageOfDefaultValue = false,
        char passwordCharacter = '\0') : base(propertyName, true, defaultValue, autoUsageOfDefaultValue)
    {
        _passwordCharacter = passwordCharacter;
    }

    public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        string? curValue = GetValue(recordForUpdate, DefaultValue);
        SetValue(recordForUpdate, Inputer.InputText(FieldName, curValue, _passwordCharacter));
        return ValueTask.CompletedTask;
    }

    public override string GetValueStatus(object? record)
    {
        string? val = GetValueOrDefault(record);
        if (val is null || _passwordCharacter == 0 || string.IsNullOrEmpty(val))
        {
            return val ?? string.Empty;
        }

        return new string(_passwordCharacter, val.Length);
    }
}
