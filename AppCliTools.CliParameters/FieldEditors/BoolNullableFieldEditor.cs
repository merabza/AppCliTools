using AppCliTools.LibDataInput;

namespace AppCliTools.CliParameters.FieldEditors;

public sealed class BoolNullableFieldEditor : FieldEditor<bool?>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public BoolNullableFieldEditor(string propertyName, bool? defaultValue = false,
        bool autoUsageOfDefaultValue = false) : base(propertyName, true, defaultValue, autoUsageOfDefaultValue)
    {
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        SetValue(recordForUpdate, Inputer.InputBool(FieldName, GetValue(recordForUpdate, DefaultValue) ?? false));
    }

    public override string GetValueStatus(object? record)
    {
        bool? val = GetValueOrDefault(record);
        return val?.ToString() ?? string.Empty;
    }
}
